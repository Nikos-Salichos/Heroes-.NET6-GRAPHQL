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
        [Route("qenerateQRCode/{qrText}")]
        public BarcodeResult? GetQrCode(string fileName, string extension, string qrText)
        {
            try
            {
                string fullPath = $"{Environment.CurrentDirectory}\\{fileName}" + "." + $"{extension}";

                GeneratedBarcode? qrImage = QRCodeWriter.CreateQrCode(qrText, 500, QRCodeWriter.QrErrorCorrectionLevel.Medium);

                if (extension.ToLower().Equals("png", StringComparison.InvariantCultureIgnoreCase))
                {
                    qrImage.SaveAsPng(fullPath);
                }
                else
                {
                    return null;
                }

                if (fullPath is not null)
                {
                    BarcodeResult Result = BarcodeReader.QuicklyReadOneBarcode(@$"{fullPath}", BarcodeEncoding.QRCode);
                    return Result;
                }

                return null;
            }
            catch (Exception exception)
            {
                _logger.LogError($"Logging {MethodBase.GetCurrentMethod()} " + exception.Message);
                return null;
            }
        }

        //TODO https://ironsoftware.com/csharp/barcode/tutorials/csharp-qr-code-generator/
    }
}
