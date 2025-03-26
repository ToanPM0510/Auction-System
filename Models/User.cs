using System.ComponentModel.DataAnnotations;

namespace AuctionSystem.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Username must be between 3 and 50 characters")]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Password hash is required")]
        public string PasswordHash { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(Admin|Seller|Buyer)$", ErrorMessage = "Role must be Admin, Seller, or Buyer")]
        public string Role { get; set; } = "Buyer";

        public List<Bid> Bids { get; set; } = new();
        public List<AutoBid> AutoBids { get; set; } = new();
        public List<Auction> Auctions { get; set; } = new();
    }
}