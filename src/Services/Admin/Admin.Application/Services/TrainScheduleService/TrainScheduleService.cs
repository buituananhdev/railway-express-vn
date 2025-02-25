using Admin.Application.Dtos;
using Admin.Application.Repositories;
using Admin.Domain.Entities;
using Admin.Domain.Specifications;
using AutoMapper;
using Common.Domain.Specifications;
using System.Linq.Expressions;

namespace Admin.Application.Services;
public class TrainScheduleService : ITrainScheduleService
{
    private readonly IAdminUnitOfWork _adminUnitOfWork;
    private readonly IMapper _mapper;

    public TrainScheduleService(IAdminUnitOfWork adminUnitOfWork, IMapper mapper)
    {
        _adminUnitOfWork = adminUnitOfWork;
        _mapper = mapper;
    }

    public async Task AddTrainScheduleAsync(AddTrainScheduleDto trainScheduleDto)
    {
        var schedule = _mapper.Map<TrainSchedule>(trainScheduleDto);
        await _adminUnitOfWork.TrainScheduleRepository.AddAsync(schedule);
        await _adminUnitOfWork.SaveChangesAsync();
    }

    public async Task<List<TrainScheduleDto>> GetTrainSchedulesAsync(GetTrainSchedulesDto getTrainSchedulesDto)
    {
        var includes = new List<Expression<Func<TrainSchedule, object>>>
        {
            schedule => schedule.DepartureStation!,
            schedule => schedule.ArrivalStation!,
            schedule => schedule.Train!
        };

        var specification = new AndSpecification<TrainSchedule>(
            new DepartureStationIdSpecification(getTrainSchedulesDto.DepartureStationId),
            new ArrivalStationIdSpecification(getTrainSchedulesDto.ArrivalStationId)
        );

        var schedules = await _adminUnitOfWork.TrainScheduleRepository
            .ToListAsync(
                spec: specification,
                includes: includes
            );

        var trainScheduleDtos = new List<TrainScheduleDto>();

        foreach (var schedule in schedules)
        {
            decimal basePrice = CalculateBasePrice(schedule.Distance);

            var daysUntilDeparture = (getTrainSchedulesDto.DepartureTime - DateTime.Now).TotalDays;
            decimal priceMultiplier = 1m;

            if (daysUntilDeparture <= 7)
            {
                priceMultiplier += 0.5m;
            }

            if (getTrainSchedulesDto.ReturnTime.HasValue)
            {
                priceMultiplier -= 0.2m;
            }

            decimal economyPrice = basePrice * priceMultiplier;
            decimal businessPrice = economyPrice * 1.3m;

            trainScheduleDtos.Add(new TrainScheduleDto
            {
                Id = schedule.Id,
                DepartureStationId = schedule.DepartureStationId,
                ArrivalStationId = schedule.ArrivalStationId,
                DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime,
                Duration = (schedule.ArrivalTime - schedule.DepartureTime).Minutes,
                Distance = schedule.Distance,
                FromPrice = Math.Round(economyPrice, 2),
                ToPrice = Math.Round(businessPrice, 2),
                Train = _mapper.Map<TrainDto>(schedule.Train)
            });
        }

        return trainScheduleDtos;
    }

    private decimal CalculateBasePrice(int distance)
    {
        if (distance < 10) return distance * 2000m;
        if (distance <= 25) return distance * 1700m;
        if (distance <= 50) return distance * 1500m;
        if (distance <= 100) return distance * 1300m;
        if (distance <= 500) return distance * 1200m;
        return distance * 1000m;
    }
}
