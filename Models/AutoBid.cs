using System.ComponentModel.DataAnnotations;

namespace AuctionSystem.Models
{
    public class AutoBid
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Auction ID is required")]
        public int AuctionId { get; set; }

        public Auction Auction { get; set; } = null!;

        [Range(0.01, double.MaxValue, ErrorMessage = "Max amount must be greater than 0")]
        public double MaxAmount { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "Increment must be greater than 0")]
        public double Increment { get; set; }

        [Range(0, 100, ErrorMessage = "Stop percentage must be between 0 and 100")]
        public double StopPercentage { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        public User User { get; set; } = null!;
        public bool IsActive { get; set; }
    }
}