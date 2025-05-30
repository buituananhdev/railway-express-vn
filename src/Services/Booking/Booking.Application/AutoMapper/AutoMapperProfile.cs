using AutoMapper;
using Booking.Application.Dtos;
using Booking.Domain.Entities;
using Common.Protos;

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

        CreateMap<Seat, GetSeatInformationResponse>().ReverseMap();

        CreateMap<TicketSeat, TicketSeatDto>().ReverseMap();
        CreateMap<AddTicketSeatDto, TicketSeat>().ReverseMap();
        CreateMap<AddTicketSeatDto, TicketSeatDto>().ReverseMap();

        // Forward mappings (DTO -> Proto)
        CreateMap<Seat, GetSeatInformationResponse>()
            .ForMember(dest => dest.TrainCar, opt => opt.MapFrom(src => src.TrainCar))
            .ForMember(dest => dest.SeatNumber, opt => opt.MapFrom(src => src.SeatNumber))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

        CreateMap<TrainCar, TrainCarFullInformation>()
            .ForMember(dest => dest.CarNumber, opt => opt.MapFrom(src => src.CarNumber ?? 0))
            .ForMember(dest => dest.SeatType, opt => opt.MapFrom(src => (int)src.SeatType))
            .ForMember(dest => dest.Train, opt => opt.MapFrom(src => src.Train));

        CreateMap<Train, TrainFullInformation>()
            .ForMember(dest => dest.TrainName, opt => opt.MapFrom(src => src.TrainName ?? string.Empty));

        // Reverse mappings (Proto -> DTO) - only if needed
        CreateMap<GetSeatInformationResponse, Seat>()
            .ForMember(dest => dest.TrainCar, opt => opt.MapFrom(src => src.TrainCar))
            .ForMember(dest => dest.SeatNumber, opt => opt.MapFrom(src => src.SeatNumber))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

        CreateMap<TrainCarFullInformation, TrainCar>()
            .ForMember(dest => dest.CarNumber, opt => opt.MapFrom(src => src.CarNumber == 0 ? (int?)null : src.CarNumber))
            .ForMember(dest => dest.SeatType, opt => opt.MapFrom(src => src.SeatType))
            .ForMember(dest => dest.Train, opt => opt.MapFrom(src => src.Train));

        CreateMap<TrainFullInformation, Train>()
            .ForMember(dest => dest.TrainName, opt => opt.MapFrom(src => src.TrainName ?? string.Empty));

        CreateMap<BookingOrderDto, BookingOrder>().ReverseMap();

        CreateMap<AddBookingOrderDto, BookingOrder>().ReverseMap();
    }
}
