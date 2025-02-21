using AutoMapper;
using Common.Protos;
using Grpc.Core;
using UserManagement.Application.Dtos;
using UserManagement.Application.Services;

namespace UserManagement.Infrastructure.GrpcServices
{
    public class UserService : User.UserBase
    {
        private readonly IPassengerService _passengerService;
        private readonly IUserAccountService _userAccountService;
        private readonly IMapper _mapper;

        public UserService(IPassengerService passengerService, IUserAccountService userAccountService, IMapper mapper)
        {
            _passengerService = passengerService;
            _userAccountService = userAccountService;
            _mapper = mapper;
        }

        public override async Task<UserGrpcResponse> CheckUser(CheckUserRequest request, ServerCallContext context)
        {
            var user = await _passengerService.GetPassengerByEmailAsync(request.Email);
            if (user == null)
            {
                return new UserGrpcResponse
                {
                    IsSuccess = false,
                    Data = null,
                    Message = "Invalid email or password!"
                };
            }

            if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return new UserGrpcResponse
                {
                    IsSuccess = false,
                    Data = null,
                    Message = "Invalid email or password!"
                };
            }

            return new UserGrpcResponse
            {
                IsSuccess = true,
                Data = user.Id.ToString(),
                Message = "User exists and password is valid!"
            };
        }

        public override async Task<UserGrpcResponse> CreateUser(CreateUserRequest request, ServerCallContext context)
        {
            var createUserDto = _mapper.Map<AddPassengerDto>(request);
            await _passengerService.AddPassengerAsync(createUserDto);
            return new UserGrpcResponse
            {
                IsSuccess = true,
                Data = null,
                Message = "User created successfully!"
            };
        }
    }
}
