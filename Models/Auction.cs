using System.ComponentModel.DataAnnotations;

namespace AuctionSystem.Models
{
    public class Auction
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Product name is required")]
        [StringLength(100, MinimumLength = 3, ErrorMessage = "Product name must be between 3 and 100 characters")]
        public string ProductName { get; set; } = string.Empty;

        [Range(0.01, double.MaxValue, ErrorMessage = "Starting price must be greater than 0")]
        public double StartingPrice { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Current price must be greater than 0")]
        public double CurrentPrice { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Reserve price must be greater than 0")]
        public double? ReservePrice { get; set; }

        [Required(ErrorMessage = "Auction type is required")]
        [RegularExpression("^(Public|Sealed|Reverse)$", ErrorMessage = "Auction type must be Public, Sealed, or Reverse")]
        public string AuctionType { get; set; } = "Public";

        [Required(ErrorMessage = "Start time is required")]
        public DateTime StartTime { get; set; }

        [Required(ErrorMessage = "End time is required")]
        public DateTime EndTime { get; set; }

        public bool IsActive { get; set; }

        [Required(ErrorMessage = "Seller ID is required")]
        public int SellerId { get; set; }

        public User Seller { get; set; } = null!;
        public List<Bid> Bids { get; set; } = new();
        public List<AutoBid> AutoBids { get; set; } = new();
    }
}