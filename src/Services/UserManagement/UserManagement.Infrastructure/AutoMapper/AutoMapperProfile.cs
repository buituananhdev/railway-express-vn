using AutoMapper;
using Common.Protos;
using UserManagement.Application.Dtos;

namespace UserManagement.Infrastructure.AutoMapper;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<CreateUserRequest, AddPassengerDto>().ReverseMap();
    }
}
