using Admin.Application.Dtos;
using Admin.Domain.Entities;
using AutoMapper;

namespace Admin.Application.AutoMapper
{
    public class AutoMapperProfile : Profile
    {
        public AutoMapperProfile()
        {
            // Train -> TrainDto
            CreateMap<Train, TrainDto>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.TrainCars, opt => opt.MapFrom(src => src.TrainCars));

            // TrainDto -> Train
            CreateMap<TrainDto, Train>()
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.TrainCars, opt => opt.Ignore());

            // AddTrainDto -> Train
            CreateMap<AddTrainDto, Train>();

            // TrainCar -> TrainCarDto
            CreateMap<TrainCar, TrainCarDto>();

            // TrainCarDto -> TrainCar
            CreateMap<TrainCarDto, TrainCar>()
                .ForMember(dest => dest.TrainId, opt => opt.Ignore())
                .ForMember(dest => dest.Train, opt => opt.Ignore())
                .ForMember(dest => dest.Seats, opt => opt.Ignore());

            // TrainStatus -> TrainStatusDto
            CreateMap<TrainStatus, TrainStatusDto>()
                .ForMember(dest => dest.Station, opt => opt.MapFrom(src => src.Station))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status));

            // TrainStatusDto -> TrainStatus
            CreateMap<TrainStatusDto, TrainStatus>()
                .ForMember(dest => dest.TrainId, opt => opt.Ignore())
                .ForMember(dest => dest.Train, opt => opt.Ignore())
                .ForMember(dest => dest.StationId, opt => opt.Ignore());

            // TrainCar -> AddTrainCarDto
            CreateMap<TrainCar, AddTrainCarDto>();

            // AddTrainCarDto -> TrainCar
            CreateMap<AddTrainCarDto, TrainCar>()
                .ForMember(dest => dest.Train, opt => opt.Ignore())
                .ForMember(dest => dest.Seats, opt => opt.Ignore());

            // Station -> StationDto
            CreateMap<Station, StationDto>();

            // StationDto -> Station
            CreateMap<StationDto, Station>()
                .ForMember(dest => dest.DepartureTrainSchedules, opt => opt.Ignore())
                .ForMember(dest => dest.ArrivalTrainSchedules, opt => opt.Ignore());

            // Station -> AddStationDto
            CreateMap<Station, AddStationDto>();

            // AddStationDto -> Station
            CreateMap<AddStationDto, Station>()
                .ForMember(dest => dest.TrainAtStation, opt => opt.Ignore())
                .ForMember(dest => dest.DepartureTrainSchedules, opt => opt.Ignore())
                .ForMember(dest => dest.ArrivalTrainSchedules, opt => opt.Ignore());

            // TrainSchedule -> TrainScheduleDto
            CreateMap<TrainSchedule, TrainScheduleDto>()
                .ForMember(dest => dest.DepartureStation, opt => opt.MapFrom(src => src.DepartureStation))
                .ForMember(dest => dest.ArrivalStation, opt => opt.MapFrom(src => src.ArrivalStation));

            // TrainScheduleDto -> TrainSchedule
            CreateMap<TrainScheduleDto, TrainSchedule>()
                .ForMember(dest => dest.DepartureStationId, opt => opt.Ignore())
                .ForMember(dest => dest.ArrivalStationId, opt => opt.Ignore())
                .ForMember(dest => dest.DepartureStation, opt => opt.Ignore())
                .ForMember(dest => dest.ArrivalStation, opt => opt.Ignore());

            // AddTrainScheduleDto -> TrainSchedule
            CreateMap<AddTrainScheduleDto, TrainSchedule>()
                .ForMember(dest => dest.DepartureStationId, opt => opt.MapFrom(src => src.DepartureStationId))
                .ForMember(dest => dest.ArrivalStationId, opt => opt.MapFrom(src => src.ArrivalStationId));
        }
    }
}
