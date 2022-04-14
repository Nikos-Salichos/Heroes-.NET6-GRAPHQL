using HeroesAPI.Entitites.Models;
using IronBarCode;
using System.Runtime.CompilerServices;

namespace HeroesAPI.Repository
{
    public class BarcodeRepository : IBarcodeRepository
    {
        private readonly ILogger<BarcodeRepository> _logger;

        public BarcodeRepository(ILogger<BarcodeRepository> logger)
        {
            _logger = logger;
        }

        public byte[] GenerateBarcode(BarcodeModel barcodeModel)
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
                throw new ApplicationException(GetCurrentMethod() + " " + GetType().Name + " failed, extension is not correct");
            }

            if (fullPath is not null)
            {
                byte[] byteArray = System.IO.File.ReadAllBytes(fullPath);
                return byteArray;
            }
            else
            {
                throw new ApplicationException(GetCurrentMethod() + " " + GetType().Name + " failed to find image");
            }
        }


        public string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }

    }
}
