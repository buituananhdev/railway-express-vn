using AutoMapper;
using Common.Protos;
using UserManagement.Application.Dtos;
using UserManagement.Domain.Entities;

namespace UserManagement.Application.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Passenger, PassengerDto>().ReverseMap();
            CreateMap<Passenger, AddPassengerDto>().ReverseMap();

            CreateMap<CreateUserRequest, AddPassengerDto>().ReverseMap();
        }
    }
}
