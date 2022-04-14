using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarcodeController : ControllerBase
    {

        private readonly ILogger<QRCodeController> _logger;

        private readonly IBarcodeRepository _barcodeRepository;

        public BarcodeController(ILogger<QRCodeController> logger, IBarcodeRepository barcodeRepository)
        {
            _logger = logger;
            _barcodeRepository = barcodeRepository;
        }

        [HttpPost]
        [Route("qenerateBarcode/barcodeText")]
        public IActionResult CreateBarcode(BarcodeModel barcodeModel)
        {
            byte[]? byteArray = _barcodeRepository.GenerateBarcode(barcodeModel);

            if (byteArray == null)
            {
                throw new KeyNotFoundException(_barcodeRepository.GetCurrentMethod() + " " + GetType().Name + " failed, extension is not correct");
            }

            return File(byteArray, $"image/{barcodeModel.Extension}");

        }


    }
}
