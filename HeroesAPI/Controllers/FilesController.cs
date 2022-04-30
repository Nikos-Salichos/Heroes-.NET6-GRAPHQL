using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.StaticFiles;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FilesController : ControllerBase
    {
        private readonly FileExtensionContentTypeProvider _fileExtensionContextTypeProvider;

        public FilesController(FileExtensionContentTypeProvider fileExtensionContentTypeProvider)
        {
            _fileExtensionContextTypeProvider = fileExtensionContentTypeProvider
                ?? throw new System.ArgumentNullException(nameof(fileExtensionContentTypeProvider));
        }

        [HttpGet("{fileId}")]
        public ActionResult GetFile(string fileId)
        {
            var pathToFile = @"C:\Users\Nikos\source\repos\HeroesAPI\HeroesAPI\logs20220430.txt";

            if (!System.IO.File.Exists(pathToFile))
            {
                return NotFound();
            }

            if (!_fileExtensionContextTypeProvider.TryGetContentType(pathToFile, out var contentType)) { }
            {
                contentType = "application/octet-stream";
            }

            var bytes = System.IO.File.ReadAllBytes(pathToFile);
            return File(bytes, contentType, Path.GetFileName(pathToFile));

        }
    }
}
