using System.ComponentModel.DataAnnotations;

namespace Authentication.Models
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; }

        [Required]
        [StringLength(100)]
        public string Password { get; set; }

        [StringLength(50)]
        public string Role { get; set; } = "User";

        [StringLength(100)]
        public string Email { get; set; }
    }
}