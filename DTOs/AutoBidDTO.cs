using System.ComponentModel.DataAnnotations;

namespace AuctionSystem.DTOs
{
    public class AutoBidDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Auction ID is required")]
        public int AuctionId { get; set; }

        [Range(0.01, double.MaxValue)]
        public double MaxAmount { get; set; }

        [Range(0.01, double.MaxValue)]
        public double Increment { get; set; }

        [Range(0, 100)]
        public double StopPercentage { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        public bool IsActive { get; set; }
    }
}