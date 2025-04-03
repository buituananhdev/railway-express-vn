using AutoMapper;
using Common.Protos;
using UserManagement.Application.Dtos;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.AutoMapper;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<Passenger, PassengerDto>()
            .ForMember(dest => dest.Role, opt => opt.MapFrom(src => src.UserAccount.Role))
            .ReverseMap();
        CreateMap<Passenger, AddPassengerDto>().ReverseMap();
        CreateMap<Passenger, UpdatePassengerDto>().ReverseMap();
        CreateMap<CreateUserRequest, AddPassengerDto>().ReverseMap();

        CreateMap<UserAccountDto, UserAccount>().ReverseMap();
    }
}
