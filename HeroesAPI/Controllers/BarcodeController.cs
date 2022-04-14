using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Mvc;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BarcodeController : ControllerBase
    {

        private readonly ILogger<QRCodeController> _logger;


        private readonly IUnitOfWorkRepository _unitOfWorkRepository;

        public BarcodeController(ILogger<QRCodeController> logger, IUnitOfWorkRepository unitOfWorkRepository)
        {
            _logger = logger;
            _unitOfWorkRepository = unitOfWorkRepository;
        }

        [HttpPost]
        [Route("qenerateBarcode/barcodeText")]
        public IActionResult CreateBarcode(BarcodeModel barcodeModel)
        {
            byte[]? byteArray = _unitOfWorkRepository.BarcodeRepository.GenerateBarcode(barcodeModel);

            if (byteArray == null)
            {
                throw new KeyNotFoundException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " failed, extension is not correct");
            }

            return File(byteArray, $"image/{barcodeModel.Extension}");

        }


    }
}
