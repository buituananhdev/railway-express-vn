using Auth.Application.Services;
using Microsoft.AspNetCore.Mvc;
using Passenger.API;

namespace ApiGateway.Controllers
{
    [Route("api/auth")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IGreeterGrpcService _greeterGrpcService;

        public AuthController(IGreeterGrpcService greeterGrpcService)
        {
            _greeterGrpcService = greeterGrpcService;
        }

        [HttpGet]
        public async Task<IActionResult> Greeter(string name)
        {
            var response = await _greeterGrpcService.SayHelloAsync(name, CancellationToken.None);
            return Ok(response);
        }
    }
}
