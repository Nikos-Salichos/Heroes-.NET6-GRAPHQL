using System.ComponentModel.DataAnnotations;

namespace HeroesAPI.Entitites.Models
{
    public class QRCodeModel
    {
        [Display(Name = "Enter QRCode Text")]
        public string? QRCodeText { get; set; }

    }
}
