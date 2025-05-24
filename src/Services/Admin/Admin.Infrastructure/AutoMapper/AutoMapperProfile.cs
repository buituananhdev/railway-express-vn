using Admin.Application.Dtos;
using Admin.Domain.Enums;
using AutoMapper;
using Common.Protos;

namespace Admin.Infrastructure.AutoMapper;

public class AutoMapperProfile : Profile
{
    public AutoMapperProfile()
    {
        // Forward mappings (DTO -> Proto)
        CreateMap<SeatFullInformationDto, GetSeatInformationResponse>()
            .ForMember(dest => dest.TrainCar, opt => opt.MapFrom(src => src.TrainCar))
            .ForMember(dest => dest.SeatNumber, opt => opt.MapFrom(src => src.SeatNumber))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (int)src.Status));

        CreateMap<TrainCarFullInformationDto, TrainCarFullInformation>()
            .ForMember(dest => dest.CarNumber, opt => opt.MapFrom(src => src.CarNumber ?? 0))
            .ForMember(dest => dest.SeatType, opt => opt.MapFrom(src => (int)src.SeatType))
            .ForMember(dest => dest.Train, opt => opt.MapFrom(src => src.Train));

        CreateMap<TrainFullInformationDto, TrainFullInformation>()
            .ForMember(dest => dest.TrainName, opt => opt.MapFrom(src => src.TrainName ?? string.Empty));

        // Reverse mappings (Proto -> DTO) - only if needed
        CreateMap<GetSeatInformationResponse, SeatFullInformationDto>()
            .ForMember(dest => dest.TrainCar, opt => opt.MapFrom(src => src.TrainCar))
            .ForMember(dest => dest.SeatNumber, opt => opt.MapFrom(src => src.SeatNumber))
            .ForMember(dest => dest.Status, opt => opt.MapFrom(src => (SeatStatusEnum)src.Status));

        CreateMap<TrainCarFullInformation, TrainCarFullInformationDto>()
            .ForMember(dest => dest.CarNumber, opt => opt.MapFrom(src => src.CarNumber == 0 ? (int?)null : src.CarNumber))
            .ForMember(dest => dest.SeatType, opt => opt.MapFrom(src => (SeatType)src.SeatType))
            .ForMember(dest => dest.Train, opt => opt.MapFrom(src => src.Train));

        CreateMap<TrainFullInformation, TrainFullInformationDto>()
            .ForMember(dest => dest.TrainName, opt => opt.MapFrom(src => src.TrainName ?? string.Empty));
    }
}
