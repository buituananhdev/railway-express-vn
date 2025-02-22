using Auth.Application.Payloads;
using Auth.Application.Dtos;

namespace Auth.Application.Services;
public interface IAuthService
{
    Task<TokenPayload> LoginAsync(LoginDto loginDto);
    Task RegisterAsync(RegisterDto registrationDto);
}
