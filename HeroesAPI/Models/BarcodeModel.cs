using System.ComponentModel.DataAnnotations;

namespace HeroesAPI.Models
{
    public class BarcodeModel
    {
        [Required(ErrorMessage = "Text required")]
        public string Text { get; set; }

        [Required(ErrorMessage = "Extension required")]
        public string Extension { get; set; }

        [Required(ErrorMessage = "MaxWidth required")]
        [Range(500, 5000, ErrorMessage = "MaxWidth must be integer between 500 and 2500")]
        public int MaxWidth { get; set; }

        [Required(ErrorMessage = "MaxHeight required")]
        [Range(500, 5000, ErrorMessage = "MaxHeight must be integer between 500 and 2500")]
        public int MaxHeight { get; set; }

    }
}
