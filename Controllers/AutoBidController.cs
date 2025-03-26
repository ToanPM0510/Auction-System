using AuctionSystem.DTOs;
using AuctionSystem.Models;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.Security.Claims;

namespace AuctionSystem.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AutoBidController : ControllerBase
    {
        private readonly AuctionDbContext _context;
        private readonly IMapper _mapper;

        public AutoBidController(AuctionDbContext context, IMapper mapper)
        {
            _context = context;
            _mapper = mapper;
        }

        [Authorize(Policy = "BuyerOnly")]
        [HttpPost]
        public async Task<IActionResult> CreateAutoBid([FromBody] AutoBidRequest model)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);

            var auction = await _context.Auctions.FindAsync(model.AuctionId);
            if (auction == null || !auction.IsActive || auction.EndTime < DateTime.UtcNow)
                return BadRequest("Auction not found or ended");

            var autoBid = new AutoBid
            {
                AuctionId = model.AuctionId,
                MaxAmount = model.MaxAmount,
                Increment = model.Increment,
                StopPercentage = model.StopPercentage,
                UserId = int.Parse(User.FindFirst(ClaimTypes.Name)?.Value ?? "0"),
                IsActive = true
            };

            _context.AutoBids.Add(autoBid);
            await _context.SaveChangesAsync();

            var autoBidDto = _mapper.Map<AutoBidDTO>(autoBid);
            return Ok(autoBidDto);
        }

        [HttpGet("{auctionId}")]
        public async Task<IActionResult> GetAutoBids(int auctionId)
        {
            var autoBids = await _context.AutoBids
                .Where(ab => ab.AuctionId == auctionId)
                .ToListAsync();

            var autoBidDtos = _mapper.Map<List<AutoBidDTO>>(autoBids);
            return Ok(autoBidDtos);
        }
    }

    public class AutoBidRequest
    {
        [Required(ErrorMessage = "Auction ID is required")]
        public int AuctionId { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Max amount must be greater than 0")]
        public double MaxAmount { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Increment must be greater than 0")]
        public double Increment { get; set; }

        [Range(0, 100, ErrorMessage = "Stop percentage must be between 0 and 100")]
        public double StopPercentage { get; set; }
    }
}