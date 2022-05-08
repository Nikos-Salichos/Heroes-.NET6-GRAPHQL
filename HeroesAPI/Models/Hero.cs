using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace HeroesAPI.Models
{
    public class Hero
    {
        [Key]
        public int Id { get; set; }

        [Required(AllowEmptyStrings = false, ErrorMessage = "Name is Required")]
        [StringLength(50, ErrorMessage = "Name can't be longer than 50 characters")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "FirstName is Required")]
        [StringLength(50, ErrorMessage = "Name can't be longer than 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "LastName is Required")]
        [StringLength(50, ErrorMessage = "Name can't be longer than 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Place is Required")]
        [StringLength(50, ErrorMessage = "Name can't be longer than 50 characters")]
        public string Place { get; set; } = string.Empty;

        [NotMapped]
        public IFormFile? Image { get; set; }

        public string? ImageUrl { get; set; }

    }
}
