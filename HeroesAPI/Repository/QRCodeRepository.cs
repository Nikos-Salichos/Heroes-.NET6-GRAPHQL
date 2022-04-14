using HeroesAPI.Entitites.Models;

namespace HeroesAPI.Repository
{
    public class QRCodeRepository : IQRCodeRepository
    {
        private readonly ILogger<QRCodeRepository> _logger;

        public QRCodeRepository(ILogger<QRCodeRepository> logger)
        {
            _logger = logger;
        }

        public Task<ApiResponse> CreateQRCodeWithLogo(QRCodeModel qrCodeModel)
        {
            throw new NotImplementedException();
        }

        public Task<ApiResponse> ReadQRCode(IFormFile qrImage)
        {
            throw new NotImplementedException();
        }
    }
}
