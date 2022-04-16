using HeroesAPI.Entitites.Models;
using IronBarCode;
using Microsoft.AspNetCore.Mvc;
using System.Drawing;
using System.Reflection;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class QRCodeController : ControllerBase
    {
        private readonly ILogger<QRCodeController> _logger;

        private readonly IUnitOfWorkRepository _unitOfWorkRepository;

        public QRCodeController(ILogger<QRCodeController> logger, IUnitOfWorkRepository unitOfWorkRepository)
        {
            _logger = logger;
            _unitOfWorkRepository = unitOfWorkRepository;
        }

        [HttpPost]
        [Route("qenerateQRCode/qrTextWithLogo")]
        public async Task<ActionResult> CreateQRCodeWithLogo([FromForm] QRCodeModel qrCodeModel)
        {
            try
            {


                if (qrCodeModel.Logo != null
                     && !qrCodeModel.Logo.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                     && !qrCodeModel.Logo.FileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                     && !qrCodeModel.Logo.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                   )
                {
                    throw new ApplicationException(GetType().Name + " " + "file is not an image");
                }

                Guid imageName = Guid.NewGuid();
                string pathToSaveImage = $"{Environment.CurrentDirectory}\\{imageName}" + ".png";

                GeneratedBarcode? qrImage;
                if (qrCodeModel.Logo != null)
                {
                    using (FileStream fileStream = System.IO.File.Create(pathToSaveImage))
                    {
                        await qrCodeModel.Logo.CopyToAsync(fileStream);
                        fileStream.Flush();
                    }
                    qrImage = QRCodeWriter.CreateQrCodeWithLogo(qrCodeModel.ScannedText, pathToSaveImage, qrCodeModel.Size);
                }
                else
                {
                    qrImage = QRCodeWriter.CreateQrCode(qrCodeModel.ScannedText, qrCodeModel.Size, QRCodeWriter.QrErrorCorrectionLevel.Medium);
                }

                qrImage.AddAnnotationTextAboveBarcode(qrCodeModel.TextAboveCode);

                if (qrCodeModel.ShowScannedTextBelowCode)
                {
                    qrImage.AddBarcodeValueTextBelowBarcode();
                }

                Guid qrCodeName = Guid.NewGuid();
                string fullPath = $"{Environment.CurrentDirectory}\\{qrCodeModel.ScannedText}" + $"{qrCodeName}" + $".{qrCodeModel.Extension}";

                if (qrCodeModel.Extension.ToLower().Equals("png", StringComparison.InvariantCultureIgnoreCase))
                {
                    qrImage.SaveAsPng(fullPath);
                }
                else if (qrCodeModel.Extension.ToLower().Equals("jpeg", StringComparison.InvariantCultureIgnoreCase))
                {
                    qrImage.SaveAsJpeg(fullPath);
                }
                else if (qrCodeModel.Extension.ToLower().Equals("gif", StringComparison.InvariantCultureIgnoreCase))
                {
                    qrImage.SaveAsGif(fullPath);
                }
                else
                {
                    throw new ApplicationException(GetType().Name + " " + "failed, extension is not correct");
                }

                if (fullPath is not null)
                {
                    byte[] byteArray = System.IO.File.ReadAllBytes(fullPath);
                    return File(byteArray, $"image/{qrCodeModel.Extension}");
                }
                else
                {
                    throw new ApplicationException(GetType().Name + " " + "failed to find image");
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " " + exception.Message);
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " " + exception.Message);
            }

        }

        [HttpPost]
        [Route("readQRImage/qrImage")]
        public async Task<ActionResult> ReadQRCode(IFormFile qrImage)
        {
            try
            {

                if (!qrImage.FileName.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                     && !qrImage.FileName.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase)
                     && !qrImage.FileName.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                   )
                {
                    throw new ApplicationException(MethodBase.GetCurrentMethod() + " " + GetType().Name + " " + "file is not an image");
                }

                using (MemoryStream? memoryStream = new MemoryStream())
                {
                    await qrImage.CopyToAsync(memoryStream);
                    Bitmap bitmap = new Bitmap(memoryStream);

                    BarcodeResult barcodeResult = BarcodeReader.QuicklyReadOneBarcode(bitmap);

                    if (barcodeResult is not null)
                    {
                        return Ok(barcodeResult.Text);
                    }
                    else
                    {
                        throw new ApplicationException(MethodBase.GetCurrentMethod() + " " + GetType().Name + " " + "failed to find text");
                    }
                }
            }
            catch (Exception exception)
            {
                _logger.LogError(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " " + exception.Message);
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " " + exception.Message);
            }
        }


    }
}
