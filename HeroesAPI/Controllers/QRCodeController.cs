using IronBarCode;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QRCodeController : ControllerBase
    {
        private readonly ILogger<QRCodeController> _logger;

        public QRCodeController(ILogger<QRCodeController> logger)
        {
            _logger = logger;
        }

        [HttpGet]
        [Route("retrieveImage/{fileName}")]
        public IActionResult GetQRImage(string fileName, string extension)
        {
            try
            {
                string fullPath = $"{Environment.CurrentDirectory}\\{fileName}" + "." + $"{extension}";

                if (!string.IsNullOrWhiteSpace(fileName) && System.IO.File.Exists(fullPath))
                {
                    byte[] byteArray = System.IO.File.ReadAllBytes(fullPath);
                    return File(byteArray, $"image/{extension}");
                }

                return NotFound("Image not found");
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

        [HttpPost]
        [Route("qenerateQRCode/{qrText}")]
        public IActionResult CreateQRCode(string fileName, string extension, string qrText)
        {
            try
            {
                string fullPath = $"{Environment.CurrentDirectory}\\{fileName}" + "." + $"{extension}";

                GeneratedBarcode? qrImage = QRCodeWriter.CreateQrCode(qrText, 500, QRCodeWriter.QrErrorCorrectionLevel.Medium);

                if (extension.ToLower().Equals("png", StringComparison.InvariantCultureIgnoreCase))
                {
                    qrImage.SaveAsPng(fullPath);
                }
                else if (extension.ToLower().Equals("jpeg", StringComparison.InvariantCultureIgnoreCase))
                {
                    qrImage.SaveAsJpeg(fullPath);
                }
                else if (extension.ToLower().Equals("html", StringComparison.InvariantCultureIgnoreCase))
                {
                    qrImage.SaveAsHtmlFile(fullPath);
                }
                else
                {
                    return BadRequest();
                }

                if (fullPath is not null)
                {
                    byte[] byteArray = System.IO.File.ReadAllBytes(fullPath);
                    return File(byteArray, $"image/{extension}");
                }

                return BadRequest();
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return BadRequest();
            }
        }

    }
}
