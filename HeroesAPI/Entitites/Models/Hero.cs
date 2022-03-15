using System.ComponentModel.DataAnnotations;

namespace HeroesAPI.Entities.Models
{
    public class Hero
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Name is Required")]
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
    }
}
