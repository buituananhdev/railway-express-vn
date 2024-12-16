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
        }
    }
}
