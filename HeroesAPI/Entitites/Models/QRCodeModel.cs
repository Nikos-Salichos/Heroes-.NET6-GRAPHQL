using System.ComponentModel.DataAnnotations;

namespace HeroesAPI.Entitites.Models
{
    public class QRCodeModel
    {
        [Required(ErrorMessage = "Enter QRCode text")]
        public string ScannedText { get; set; }

        [Required(ErrorMessage = "Enter QRCode logo")]
        public string Extension { get; set; }

        [Display(Name = "Enter QRCode Logo")]
        public IFormFile? Logo { get; set; }

        [Display(Name = "Enter the text you want to show above code")]
        public string? TextAboveCode { get; set; }

        [Required(ErrorMessage = "ShowScannedTextBelowCode required")]
        [Display(Name = "Show scanned text below code")]
        public bool ShowScannedTextBelowCode { get; set; }

        [Required(ErrorMessage = "Size required")]
        [Range(500, 5000, ErrorMessage = "Size must be integer between 500 and 2500")]
        [Display(Name = "Size of qrcode")]
        public int Size { get; set; }

    }
}
