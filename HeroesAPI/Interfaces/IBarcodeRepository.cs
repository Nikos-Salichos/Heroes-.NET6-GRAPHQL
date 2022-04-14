using HeroesAPI.Entitites.Models;
using System.Runtime.CompilerServices;

namespace HeroesAPI.Interfaces
{
    public interface IBarcodeRepository
    {
        byte[] GenerateBarcode(BarcodeModel barcodeModel);
        public string GetCurrentMethod([CallerMemberName] string callerName = "")
        {
            return callerName;
        }
    }
}
