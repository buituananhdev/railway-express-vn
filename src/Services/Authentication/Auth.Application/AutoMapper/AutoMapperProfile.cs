using Auth.Application.Dtos;
using AutoMapper;
using Common.Protos;

namespace Auth.Application.AutoMapper;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<LoginDto, CheckUserRequest>().ReverseMap();

        CreateMap<RegisterDto, CreateUserRequest>().ReverseMap();
    }
}
