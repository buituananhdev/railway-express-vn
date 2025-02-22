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
            var basePrice = 100000m; // Base price.
            var daysUntilDeparture = (getTrainSchedulesDto.DepartureTime - DateTime.Now).TotalDays;

            // Pricing formula:
            // - Price increases if the departure date is near the current date.
            // - Price decreases if it is a round trip.
            // - Price increases with the number of passengers.
            var priceMultiplier = 1m;
            if (daysUntilDeparture <= 7) // If the departure date is within the next 7 days.
            {
                priceMultiplier += 0.5m; // Increase price by 50%.
            }

            if (getTrainSchedulesDto.ReturnTime.HasValue) // If it is a round trip.
            {
                priceMultiplier -= 0.2m; // Decrease price by 20%.
            }

            priceMultiplier += getTrainSchedulesDto.NumberOfPassengers * 0.1m; // Increase price by 10% per passenger.

            var finalPrice = basePrice * priceMultiplier;

            trainScheduleDtos.Add(new TrainScheduleDto
            {
                Id = schedule.Id,
                DepartureStationId = schedule.DepartureStationId,
                ArrivalStationId = schedule.ArrivalStationId,
                DepartureTime = schedule.DepartureTime,
                ArrivalTime = schedule.ArrivalTime,
                Duration = (schedule.ArrivalTime - schedule.DepartureTime).Minutes,
                Price = Math.Round(finalPrice, 2),
                Train = _mapper.Map<TrainDto>(schedule.Train)
            });
        }

        return trainScheduleDtos;
    }
}
