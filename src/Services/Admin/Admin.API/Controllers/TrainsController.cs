using Admin.Application.Dtos;
using Admin.Application.Services.TrainService;
using Microsoft.AspNetCore.Mvc;

namespace Admin.API.Controllers
{
    [Route("v1/trains")]
    [ApiController]
    public class TrainsController : ControllerBase
    {
        private readonly ITrainService _trainService;

        public TrainsController(ITrainService trainService)
        {
            _trainService = trainService;
        }

        [HttpPost]
        public async Task<IActionResult> AddTrain([FromBody] AddTrainDto trainDto)
        {
            await _trainService.AddTrainAsync(trainDto);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> GetTrains()
        {
            var trains = await _trainService.GetTrainsAsync();
            return Ok(trains);
        }
    }
}
