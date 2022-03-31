using HeroesAPI.Entitites.Models;
using IronBarCode;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

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
            try
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
                    return BadRequest("Failed, extension is not correct");
                }

                if (fullPath is not null)
                {
                    byte[] byteArray = System.IO.File.ReadAllBytes(fullPath);
                    return File(byteArray, $"image/{barcodeModel.Extension}");
                }

                return Ok("Success");
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }
    }
}
