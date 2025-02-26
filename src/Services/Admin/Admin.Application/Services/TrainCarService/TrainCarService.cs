using Admin.Application.Dtos;
using Admin.Application.Repositories;
using Admin.Domain.Entities;
using Admin.Domain.Enums;
using Admin.Domain.Specifications;
using AutoMapper;
using Common.Application.Interfaces;
using Common.Application.Services;

namespace Admin.Application.Services;
public class TrainCarService : BaseService<TrainCar, AddTrainCarDto, AddTrainCarDto, TrainCarDto>, ITrainCarService
{
    private readonly IAdminUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;
    private readonly ISeatService _seatService;
    public TrainCarService(
        ITrainCarRepository repository,
        IAdminUnitOfWork unitOfWork,
        IMapper mapper,
        IPaginationService paginationService,
        ISeatService seatService
        ) : base(repository, unitOfWork, mapper, paginationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _seatService = seatService;
    }

    public async Task<List<TrainCarDto>> GetTrainCarsAndPriceAsync(Guid trainId, Guid scheduleId, DateTime journeyDate)
    {
        try
        {
            var specification = new TrainIdSpecification(trainId);
            var trainCars = await _unitOfWork.TrainCarRepository.ToListAsync(specification);

            var result = _mapper.Map<List<TrainCarDto>>(trainCars);
            foreach (var trainCarDto in result)
            {

                var (fromPrice, toPrice) = await CalculateTrainCarPricesAsync(trainCarDto.Id, scheduleId, journeyDate);

                trainCarDto.FromPrice = fromPrice;
                trainCarDto.ToPrice = toPrice;

                trainCarDto.AvailableSeats = await _seatService.GetAvailableSeatsAsync(trainCarDto.Id, scheduleId, journeyDate);
            }

            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task<(decimal FromPrice, decimal ToPrice)> CalculateTrainCarPricesAsync(Guid trainCarId, Guid scheduleId, DateTime journeyDate)
    {

        var schedule = await _unitOfWork.TrainScheduleRepository
            .GetByIdAsync(scheduleId);

        var trainCar = await _unitOfWork.TrainCarRepository.GetByIdAsync(trainCarId);

        decimal basePrice = CalculateBasePrice(schedule.Distance);

        var daysUntilDeparture = (journeyDate - DateTime.Now).TotalDays;

        decimal priceMultiplier = 1m;
        if (daysUntilDeparture <= 7)
        {
            priceMultiplier += 0.5m;
        }

        bool isBusinessClass = trainCar.SeatType == SeatType.Business;

        decimal economyPrice = basePrice * priceMultiplier;
        decimal businessPrice = economyPrice * 1.3m;

        decimal fromPrice, toPrice;

        if (isBusinessClass)
        {
            fromPrice = businessPrice;
            toPrice = businessPrice * 1.2m;
        }
        else
        {
            fromPrice = economyPrice;
            toPrice = economyPrice * 1.2m;
        }

        fromPrice = Math.Round(fromPrice, 2);
        toPrice = Math.Round(toPrice, 2);

        return (fromPrice, toPrice);
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
