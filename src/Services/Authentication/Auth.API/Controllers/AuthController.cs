using Auth.Application.Dtos;
using Auth.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace Auth.API.Controllers;
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IGreeterGrpcService _greeterGrpcService;
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService, IGreeterGrpcService greeterGrpcService)
    {
        _greeterGrpcService = greeterGrpcService;
        _authService = authService;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginDto request)
    {
        var response = await _authService.LoginAsync(request);
        return Ok(response);
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterDto request)
    {
        await _authService.RegisterAsync(request);
        return Ok();
    }

    [HttpGet]
    public async Task<IActionResult> Greeter(string name)
    {
        var response = await _greeterGrpcService.SayHelloAsync(name, CancellationToken.None);
        return Ok(response);
    }
}
