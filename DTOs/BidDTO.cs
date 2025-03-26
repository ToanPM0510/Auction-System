using System.ComponentModel.DataAnnotations;

namespace AuctionSystem.DTOs
{
    public class BidDTO
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Auction ID is required")]
        public int AuctionId { get; set; }

        [Range(0.01, double.MaxValue)]
        public double Amount { get; set; }

        [Required(ErrorMessage = "User ID is required")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "Bid time is required")]
        public DateTime BidTime { get; set; }

        public bool IsAuto { get; set; }
    }
}