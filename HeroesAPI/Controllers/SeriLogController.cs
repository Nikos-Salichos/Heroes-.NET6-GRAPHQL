using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SeriLogController : ControllerBase
    {

        private readonly IUnitOfWorkRepository _unitOfWorkRepository;


        public SeriLogController(IUnitOfWorkRepository unitOfWorkRepository)
        {
            _unitOfWorkRepository = unitOfWorkRepository;
        }

        [Route("GetAllLogs")]
        [HttpGet]
        [Authorize(Roles = UserRole.Admin)]
        public async Task<IActionResult> GetAllLogs()
        {
            IEnumerable<SeriLogModel>? logs = await _unitOfWorkRepository.SeriLogRepository.GetAllLogsAsync();

            if (logs is null)
            {
                throw new ApplicationException(_unitOfWorkRepository.GetCurrentMethod() + " " + GetType().Name + " " + "failed to get logs");
            }

            return Ok(logs);

        }

    }
}
