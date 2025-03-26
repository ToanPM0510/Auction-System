using AuctionSystem.Models;
using AuctionSystem.Hubs;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StackExchange.Redis;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using AuctionSystem.DTOs;

namespace AuctionSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuctionController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;
        private readonly IDistributedCache _cache;
        private readonly IConnectionMultiplexer _redis;
        private readonly IHubContext<AuctionHub> _hubContext;

        public AuctionController(AuctionDbContext context, IMapper mapper, IDistributedCache cache, IConnectionMultiplexer redis, IHubContext<AuctionHub> hubContext)
        {
            _context = context;
            _mapper = mapper;
            _cache = cache;
            _redis = redis;
            _hubContext = hubContext;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var auctions = await _context.Auctions
                .Include(a => a.Bids)
                .ToListAsync();
            var auctionDtos = _mapper.Map<List<AuctionDto>>(auctions);
            return Ok(auctionDtos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetAuction(int id)
        {
            var auction = await _context.Auctions
                .Include(a => a.Bids)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (auction == null) return NotFound();

            string currentPriceStr = await _cache.GetStringAsync($"auction:{id}:price");
            if (!string.IsNullOrEmpty(currentPriceStr))
                auction.CurrentPrice = double.Parse(currentPriceStr);

            var auctionDto = _mapper.Map<AuctionDTO>(auction);
            return Ok(auctionDto);
        }

        [Authorize(Policy = "BuyerOnly")]
        [HttpPost("bid")]
        public async Task<IActionResult> PlaceBid([FromBody] BidRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var auction = await _context.Auctions.FindAsync(model.AuctionId);
            if (auction == null || !auction.IsActive || auction.EndTime < DateTime.UtcNow)
                return BadRequest("Auction not found or ended");

            var redisDb = _redis.GetDatabase();
            var lockKey = $"lock:auction:{model.AuctionId}";
            var lockValue = Guid.NewGuid().ToString();

            if (!await redisDb.LockTakeAsync(lockKey, lockValue, TimeSpan.FromMilliseconds(100)))
                return StatusCode(429, "Too many bids, try again");

            try
            {
                string currentPriceStr = await _cache.GetStringAsync($"auction:{model.AuctionId}:price");
                double currentPrice = string.IsNullOrEmpty(currentPriceStr) ? auction.CurrentPrice : double.Parse(currentPriceStr);

                if (model.Amount <= currentPrice) return BadRequest("Bid must be higher than current price");

                var bid = new Bid
                {
                    AuctionId = model.AuctionId,
                    Amount = model.Amount,
                    UserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0"),
                    BidTime = DateTime.UtcNow,
                    IsAuto = false
                };

                auction.CurrentPrice = bid.Amount;
                _context.Bids.Add(bid);
                await _context.SaveChangesAsync();
                await _cache.SetStringAsync($"auction:{model.AuctionId}:price", bid.Amount.ToString());

                // Gửi thông báo qua SignalR
                Console.WriteLine($"Sending BidUpdated for manual bid: AuctionId={model.AuctionId}, Amount={bid.Amount}");
                await _hubContext.Clients.Group(model.AuctionId.ToString())
                    .SendAsync("BidUpdated", new
                    {
                        AuctionId = model.AuctionId,
                        CurrentPrice = bid.Amount,
                        BidderId = bid.UserId,
                        IsAuto = bid.IsAuto,
                        BidTime = bid.BidTime.ToString("o")
                    });

                Console.WriteLine($"Manual bid placed: AuctionId={model.AuctionId}, Amount={bid.Amount}");
                await ProcessAutoBids(model.AuctionId, bid.Amount);

                return Ok(new { Message = "Bid placed successfully", BidAmount = model.Amount });
            }
            finally
            {
                await redisDb.LockReleaseAsync(lockKey, lockValue);
            }
        }

        private async Task ProcessAutoBids(int auctionId, double currentPrice)
        {
            var autoBids = await _context.AutoBids
                .Where(ab => ab.AuctionId == auctionId && ab.MaxAmount > currentPrice && ab.IsActive)
                .OrderByDescending(ab => ab.MaxAmount)
                .ToListAsync();

            Console.WriteLine($"Found {autoBids.Count} active auto-bids for AuctionId={auctionId}, CurrentPrice={currentPrice}");

            if (!autoBids.Any()) return;

            var topAutoBid = autoBids.First();
            var newPrice = currentPrice + topAutoBid.Increment;

            Console.WriteLine($"Top AutoBid: UserId={topAutoBid.UserId}, MaxAmount={topAutoBid.MaxAmount}, Increment={topAutoBid.Increment}, NewPrice={newPrice}");

            if (newPrice <= topAutoBid.MaxAmount)
            {
                await PlaceAutoBid(auctionId, newPrice, topAutoBid.UserId);
            }
            else
            {
                Console.WriteLine($"New price {newPrice} exceeds MaxAmount {topAutoBid.MaxAmount}, skipping auto-bid.");
            }
        }

        private async Task PlaceAutoBid(int auctionId, double amount, int userId)
        {
            var redisDb = _redis.GetDatabase();
            var lockKey = $"lock:auction:{auctionId}";
            var lockValue = Guid.NewGuid().ToString();

            if (!await redisDb.LockTakeAsync(lockKey, lockValue, TimeSpan.FromMilliseconds(100)))
            {
                Console.WriteLine($"Failed to acquire lock for auto-bid: AuctionId={auctionId}, Amount={amount}");
                return;
            }

            try
            {
                var auction = await _context.Auctions.FindAsync(auctionId);
                if (auction == null || !auction.IsActive || auction.EndTime < DateTime.UtcNow)
                {
                    Console.WriteLine($"Auction not valid for auto-bid: AuctionId={auctionId}");
                    return;
                }

                string currentPriceStr = await _cache.GetStringAsync($"auction:{auctionId}:price");
                double currentPrice = string.IsNullOrEmpty(currentPriceStr) ? auction.CurrentPrice : double.Parse(currentPriceStr);

                if (amount > currentPrice)
                {
                    var bid = new Bid
                    {
                        AuctionId = auctionId,
                        Amount = amount,
                        UserId = userId,
                        BidTime = DateTime.UtcNow,
                        IsAuto = true
                    };

                    auction.CurrentPrice = bid.Amount;
                    _context.Bids.Add(bid);
                    await _context.SaveChangesAsync();
                    await _cache.SetStringAsync($"auction:{auctionId}:price", bid.Amount.ToString());

                    // Gửi thông báo qua SignalR
                    Console.WriteLine($"Sending BidUpdated for auto-bid: AuctionId={auctionId}, Amount={bid.Amount}");
                    await _hubContext.Clients.Group(auctionId.ToString())
                        .SendAsync("BidUpdated", new
                        {
                            AuctionId = auctionId,
                            CurrentPrice = bid.Amount,
                            BidderId = bid.UserId,
                            IsAuto = bid.IsAuto,
                            BidTime = bid.BidTime.ToString("o")
                        });

                    Console.WriteLine($"Auto-bid placed: AuctionId={auctionId}, Amount={amount}, UserId={userId}");
                }
                else
                {
                    Console.WriteLine($"Auto-bid skipped: New amount {amount} not higher than current price {currentPrice}");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in PlaceAutoBid: {ex.Message}");
                throw;
            }
            finally
            {
                await redisDb.LockReleaseAsync(lockKey, lockValue);
            }
        }

        [Authorize(Policy = "SellerOnly")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateAuction([FromBody] AuctionCreateRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var auction = new Auction
            {
                ProductName = model.ProductName,
                StartingPrice = model.StartingPrice,
                CurrentPrice = model.StartingPrice,
                ReservePrice = model.ReservePrice,
                AuctionType = model.AuctionType,
                StartTime = DateTime.UtcNow,
                EndTime = DateTime.UtcNow.AddHours(1),
                IsActive = true,
                SellerId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0")
            };

            _context.Auctions.Add(auction);
            await _context.SaveChangesAsync();
            await _cache.SetStringAsync($"auction:{auction.Id}:price", auction.CurrentPrice.ToString());

            var auctionDto = _mapper.Map<AuctionDTO>(auction);
            return Ok(auctionDto);
        }
    }

    public class BidRequest
    {
        [Required(ErrorMessage = "Auction ID is required")]
        public int AuctionId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Bid amount must be greater than 0")]
        public double Amount { get; set; }
    }

    public class AuctionCreateRequest
    {
        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 3)]
        public string ProductName { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public double StartingPrice { get; set; }

        [Range(0.01, double.MaxValue)]
        public double? ReservePrice { get; set; }

        [Required(ErrorMessage = "Auction type is required")]
        [RegularExpression("^(Public|Sealed|Reverse)$")]
        public string AuctionType { get; set; } = "Public";
    }
}