using System.ComponentModel.DataAnnotations;

namespace HeroesAPI.Models
{
    public class UserChangePassword
    {
        [Required]
        public string Password { get; set; } = string.Empty;

        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
