using HeroesAPI.Models;

namespace HeroesAPI.Interfaces
{
    public interface IQRCodeRepository
    {
        Task<ApiResponse> CreateQRCodeWithLogo(QRCodeModel qrCodeModel);
        Task<ApiResponse> ReadQRCode(IFormFile qrImage);
    }
}
