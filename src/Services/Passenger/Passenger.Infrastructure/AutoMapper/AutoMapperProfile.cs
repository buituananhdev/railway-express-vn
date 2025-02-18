using AutoMapper;
using Common.Protos;
using Passenger.Application.Dtos;

namespace Passenger.Infrastructure.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<CreateUserRequest, AddPassengerDto>().ReverseMap();
        }
    }
}