using System.ComponentModel.DataAnnotations;

namespace AuctionSystem.Models
{
    public class Bid
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Auction ID is required")]
        public int AuctionId { get; set; }

        public Auction Auction { get; set; } = null!;

        [Range(0.01, double.MaxValue, ErrorMessage = "Bid amount must be greater than 0")]
        public double Amount { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        public User User { get; set; } = null!;

        [Required(ErrorMessage = "Bid time is required")]
        public DateTime BidTime { get; set; }

        public bool IsAuto { get; set; }
    }
}