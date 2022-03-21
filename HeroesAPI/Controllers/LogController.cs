﻿using HeroesAPI.Entitites.Models;
using Microsoft.AspNetCore.Mvc;
using System.Reflection;

namespace HeroesAPI.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LogController : ControllerBase
    {

        private readonly ISeriLogRepository _seriLogRepository;

        public LogController(ISeriLogRepository seriLogRepository)
        {
            _seriLogRepository = seriLogRepository;
        }

        [Route("GetAllLogs")]
        [HttpGet]
        public async Task<IActionResult> GetAllLogs()
        {
            try
            {
                IEnumerable<SeriLogModel>? logs = await _seriLogRepository.GetAllLogsAsync();

                if (logs is null)
                {
                    return NotFound("Logs not found");
                }

                return Ok(logs);
            }
            catch (Exception exception)
            {
                return BadRequest($"{MethodBase.GetCurrentMethod()} " + exception.Message);
            }
        }

    }
}