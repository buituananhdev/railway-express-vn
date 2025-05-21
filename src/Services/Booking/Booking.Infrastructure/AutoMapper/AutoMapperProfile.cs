using AutoMapper;
using Booking.Application.Dtos;
using Common.Protos;
using Google.Protobuf.WellKnownTypes;

namespace Booking.Infrastructure.AutoMapper;
public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Convert DateTime -> Timestamp
        CreateMap<DateTime, Timestamp>()
            .ConvertUsing(src => Timestamp.FromDateTime(src.ToUniversalTime()));

        CreateMap<DateTime?, Timestamp>()
            .ConvertUsing(src => src.HasValue ? Timestamp.FromDateTime(src.Value.ToUniversalTime()) : null);

        // Convert decimal -> DoubleValue
        CreateMap<decimal, DoubleValue>()
            .ConvertUsing(src => new DoubleValue { Value = (double)src });

        // Convert string -> StringValue
        CreateMap<string, StringValue>()
            .ConvertUsing(src => string.IsNullOrEmpty(src) ? null : new StringValue { Value = src });

        CreateMap<Guid?, StringValue>()
            .ConvertUsing(src => src.HasValue ? new StringValue { Value = src.Value.ToString() } : null);

        // Convert bool -> BoolValue
        CreateMap<bool, BoolValue>()
            .ConvertUsing(src => new BoolValue { Value = src });

        // PassengerInfo
        CreateMap<PassengerInfoDto, PassengerInfo>()
            .ForMember(dest => dest.IsMainPassenger, opt => opt.MapFrom(src => src.IsMainPassenger))
            .ForMember(dest => dest.AgeGroup, opt => opt.MapFrom(src => (int)src.AgeGroup))
            .ForMember(dest => dest.TicketId, opt => opt.MapFrom(src => src.TicketId.ToString()))
            .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.FirstName))
            .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.LastName))
            .ForMember(dest => dest.DateOfBirth, opt => opt.MapFrom(src => src.DateOfBirth))
            .ForMember(dest => dest.Email, opt => opt.MapFrom(src => src.Email))
            .ForMember(dest => dest.PhoneNumber, opt => opt.MapFrom(src => src.PhoneNumber))
            .ForMember(dest => dest.IdentityNumber, opt => opt.MapFrom(src => src.IdentityNumber));

        // TicketDto -> GetTicketInformationResponse
        CreateMap<TicketDto, GetTicketInformationResponse>()
            .ForMember(dest => dest.PassengerId, opt => opt.MapFrom(src => src.PassengerId))
            .ForMember(dest => dest.TrainId, opt => opt.MapFrom(src => src.TrainId.ToString()))
            .ForMember(dest => dest.SeatIds, opt => opt.MapFrom(src => src.SeatIds.Select(id => id.ToString())))
            .ForMember(dest => dest.TrainScheduleId, opt => opt.MapFrom(src => src.TrainScheduleId.ToString()))
            .ForMember(dest => dest.BookingDate, opt => opt.MapFrom(src => src.BookingDate))
            .ForMember(dest => dest.JourneyDate, opt => opt.MapFrom(src => src.JourneyDate))
            .ForMember(dest => dest.TotalPrice, opt => opt.MapFrom(src => src.TotalPrice))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status))
            .ForMember(dest => dest.Remarks, opt => opt.MapFrom(src => src.Remarks))
            .ForMember(dest => dest.PassengerDetails, opt => opt.MapFrom(src => src.PassengerDetails));
    }
}
