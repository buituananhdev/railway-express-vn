using Admin.Application.Dtos;
using Admin.Application.Services.TrainScheduleService;
using Microsoft.AspNetCore.Mvc;

namespace Admin.API.Controllers
{
    [Route("v1/trainschedules")]
    [ApiController]
    public class TrainSchedulesController : ControllerBase
    {
        private readonly ITrainScheduleService _trainScheduleService;

        public TrainSchedulesController(ITrainScheduleService trainScheduleService)
        {
            _trainScheduleService = trainScheduleService;
        }

        [HttpPost]
        public async Task<IActionResult> AddTrainScheduleAsync(AddTrainScheduleDto trainScheduleDto)
        {
            await _trainScheduleService.AddTrainScheduleAsync(trainScheduleDto);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetTrainSchedulesAsync()
        {
            var trainSchedules = await _trainScheduleService.GetTrainSchedulesAsync();
            return Ok(trainSchedules);
        }
    }
}
