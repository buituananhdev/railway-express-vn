using AutoMapper;
using Passenger.Application.Dtos;

namespace Passenger.Application.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            CreateMap<Passenger.Domain.Entities.Passenger, PassengerDto>().ReverseMap();
            CreateMap<Passenger.Domain.Entities.Passenger, AddPassengerDto>().ReverseMap();

        }
    }
}
