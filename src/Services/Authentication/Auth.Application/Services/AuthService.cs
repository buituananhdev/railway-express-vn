using Auth.Application.Dtos;
using Auth.Application.Payloads;
using Auth.Application.Utils;
using AutoMapper;
using Microsoft.Extensions.Configuration;
using Common.Protos;

namespace Auth.Application.Services;
public class AuthService : IAuthService
{
    private readonly User.UserClient _userGrpcClient;
    private readonly IMapper _mapper;
    private readonly IConfiguration _configuration;

    public AuthService(User.UserClient userGrpcClient, IMapper mapper, IConfiguration configuration)
    {
        _userGrpcClient = userGrpcClient;
        _mapper = mapper;
        _configuration = configuration;
    }
    public async Task<TokenPayload> LoginAsync(LoginDto loginDto)
    {
        var result = await _userGrpcClient.CheckUserAsync(_mapper.Map<CheckUserRequest>(loginDto));
        if(!result.IsSuccess)
        {
            throw new UnauthorizedAccessException(result.Message);
        }

        var userId = Guid.Parse(result.Data);
        var token = JwtUtil.GenerateAccessToken(userId, _configuration);

        return token;
    }

    public async Task RegisterAsync(RegisterDto registrationDto)
    {
        var result = await _userGrpcClient.CreateUserAsync(_mapper.Map<CreateUserRequest>(registrationDto));
        if(!result.IsSuccess)
        {
            throw new Exception(result.Message);
        }

        return;
    }
}
