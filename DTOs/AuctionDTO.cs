using System.ComponentModel.DataAnnotations;

namespace AuctionSystem.DTOs
{
    public class AuctionDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 3)]
        public string ProductName { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue)]
        public double StartingPrice { get; set; }

        [Range(0.01, double.MaxValue)]
        public double CurrentPrice { get; set; }

        [Range(0.01, double.MaxValue)]
        public double? ReservePrice { get; set; }

        [Required(ErrorMessage = "Auction type is required")]
        [RegularExpression("^(Public|Sealed|Reverse)$")]
        public string AuctionType { get; set; } = string.Empty;

        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Seller ID is required")]
        public int SellerId { get; set; }
    }
    public class AuctionDto
    {
        public int Id { get; set; }
        public string ProductName { get; set; }
        public double StartingPrice { get; set; }
        public double ReservePrice { get; set; }
        public double CurrentPrice { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public bool IsActive { get; set; }
        public string AuctionType { get; set; }
        public int SellerId { get; set; }
        public List<BidDto> Bids { get; set; }
    }

    public class BidDto
    {
        public int Id { get; set; }
        public int AuctionId { get; set; }
        public int UserId { get; set; }
        public double Amount { get; set; }
        public DateTime BidTime { get; set; }
        public bool IsAuto { get; set; }
    }
}
