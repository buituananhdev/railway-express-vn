using AutoMapper;
using Booking.Application.Dtos;
using Booking.Domain.Entities;

namespace Booking.Application.AutoMapper;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        #region Ticket
        CreateMap<Ticket, TicketDto>().ReverseMap();
        CreateMap<AddTicketDto, Ticket>().ReverseMap();
        #endregion

        #region PassengerInfo
        CreateMap<PassengerInfoDto, PassengerInfo>().ReverseMap();
        CreateMap<AddPassengerInfoDto, PassengerInfo>().ReverseMap();
        #endregion
    }
}
