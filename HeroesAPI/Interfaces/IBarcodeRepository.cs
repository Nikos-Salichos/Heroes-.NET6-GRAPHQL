using HeroesAPI.Entitites.Models;

namespace HeroesAPI.Interfaces
{
    public interface IBarcodeRepository
    {
        byte[] GenerateBarcode(BarcodeModel barcodeModel);

    }
}
