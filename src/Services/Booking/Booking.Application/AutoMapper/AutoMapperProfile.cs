using AutoMapper;
using Booking.Application.Dtos;
using Booking.Domain.Entities;

namespace Booking.Application.AutoMapper;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        CreateMap<AddTicketDto, Ticket>().ReverseMap();
    }
}
