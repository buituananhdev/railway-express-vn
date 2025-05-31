using AutoMapper;
using Booking.Application.Dtos;
using Booking.Domain.Entities;
using Common.Protos;
using Google.Protobuf.WellKnownTypes;

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

        CreateMap<GetTrainScheduleInformationResponse, TrainSchedule>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
                .ForMember(dest => dest.ArrivalStation, opt => opt.MapFrom(src => src.ArrivalStation))
                .ForMember(dest => dest.DepartureStation, opt => opt.MapFrom(src => src.DepartureStation))
                .ForMember(dest => dest.DepartureTime, opt => opt.MapFrom(src => src.DepartureTime.ToDateTime()))
                .ForMember(dest => dest.ArrivalTime, opt => opt.MapFrom(src => src.ArrivalTime.ToDateTime()));

        CreateMap<TrainSchedule, GetTrainScheduleInformationResponse>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.ArrivalStation, opt => opt.MapFrom(src => src.ArrivalStation))
                .ForMember(dest => dest.DepartureStation, opt => opt.MapFrom(src => src.DepartureStation))
                .ForMember(dest => dest.DepartureTime, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.DepartureTime.ToUniversalTime())))
                .ForMember(dest => dest.ArrivalTime, opt => opt.MapFrom(src => Timestamp.FromDateTime(src.ArrivalTime.ToUniversalTime())));

        CreateMap<Common.Protos.Station, Booking.Application.Dtos.Station>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => Guid.Parse(src.Id)))
                .ForMember(dest => dest.StationName, opt => opt.MapFrom(src => src.StationName))
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.CityName));

        CreateMap<Booking.Application.Dtos.Station, Common.Protos.Station>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id.ToString()))
                .ForMember(dest => dest.StationName, opt => opt.MapFrom(src => src.StationName))
                .ForMember(dest => dest.CityName, opt => opt.MapFrom(src => src.CityName));
    }
}
