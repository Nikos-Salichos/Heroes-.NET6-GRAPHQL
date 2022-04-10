using HeroesAPI.Entitites.Models;
using IronBarCode;
using Microsoft.AspNetCore.Mvc;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarcodeController : ControllerBase
    {

        private readonly ILogger<QRCodeController> _logger;

        public BarcodeController(ILogger<QRCodeController> logger)
        {
            _logger = logger;
        }

        [HttpPost]
        [Route("qenerateBarcode/barcodeText")]
        public IActionResult CreateQRCode(BarcodeModel barcodeModel)
        {
            Guid barcodeName = Guid.NewGuid();
            string fullPath = $"{Environment.CurrentDirectory}\\{barcodeModel.Text}" + $"{barcodeName}" + $".{barcodeModel.Extension}";

            GeneratedBarcode? qrImage = BarcodeWriter.CreateBarcode(barcodeModel.Text, BarcodeEncoding.Code128, barcodeModel.MaxWidth, barcodeModel.MaxHeight);

            if (barcodeModel.Extension.ToLower().Equals("png", StringComparison.InvariantCultureIgnoreCase))
            {
                qrImage.SaveAsPng(fullPath);
            }
            else if (barcodeModel.Extension.ToLower().Equals("jpeg", StringComparison.InvariantCultureIgnoreCase))
            {
                qrImage.SaveAsJpeg(fullPath);
            }
            else if (barcodeModel.Extension.ToLower().Equals("gif", StringComparison.InvariantCultureIgnoreCase))
            {
                qrImage.SaveAsGif(fullPath);
            }
            else
            {
                throw new ApplicationException("Failed, extension is not correct");
            }

            if (fullPath is not null)
            {
                byte[] byteArray = System.IO.File.ReadAllBytes(fullPath);
                return File(byteArray, $"image/{barcodeModel.Extension}");
            }
            else
            {
                throw new ApplicationException("Failed to find image path");
            }

        }
    }
}
