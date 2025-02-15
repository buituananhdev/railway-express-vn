using Auth.Application.Dtos;
using Auth.Application.Payloads;

namespace Auth.Application.Services
{
    public class AuthService : IAuthService
    {
        public Task<TokenPayload> LoginAsync(LoginDto loginDto)
        {
            throw new NotImplementedException();
            // call to passanger service
        }

        public Task RegisterAsync(RegisterDto registrationDto)
        {
            throw new NotImplementedException();
            // call to passanger service
        }
    }
}
