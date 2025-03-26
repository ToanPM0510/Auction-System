using System.ComponentModel.DataAnnotations;

namespace AuctionSystem.DTOs
{
    public class UserDTO
    {
        [Required(ErrorMessage = "User ID is required")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 3)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "Role is required")]
        [RegularExpression("^(Admin|Seller|Buyer)$")]
        public string Role { get; set; } = string.Empty;
    }
}