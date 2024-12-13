using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Passenger.Application.Dtos;
using Passenger.Application.Services;

namespace Passenger.API.Controllers
{
    [Route("passengers")]
    [ApiController]
    public class PassengersController : ControllerBase
    {
        private readonly IPassengerService _passengerService;
        public PassengersController(IPassengerService passengerService)
        {
            _passengerService = passengerService;
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetPassengerById(Guid id)
        {
            var passenger = await _passengerService.GetPassengerByIDAsync(id);
            return Ok(passenger);
        }

        [HttpPost]
        public async Task<IActionResult> AddPassenger([FromBody] AddPassengerDto passengerDto)
        {
            await _passengerService.AddPassengerAsync(passengerDto);
            return Ok();
        }
    }
}
