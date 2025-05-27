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
    private readonly ICacheService _cacheService;

    private const string TRAINCARS_CACHE_KEY = "traincars:train:{0}:schedule:{1}:date:{2}";
    private const int CACHE_HOURS = 24;

    public TrainCarService(
        ITrainCarRepository repository,
        IAdminUnitOfWork unitOfWork,
        IMapper mapper,
        IPaginationService paginationService,
        ICacheService cacheService
        ) : base(repository, unitOfWork, mapper, paginationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<List<TrainCarDto>> GetTrainCarsAndPriceAsync(Guid trainId, Guid scheduleId, DateTime journeyDate)
    {
        try
        {
            string cacheKey = string.Format(TRAINCARS_CACHE_KEY,
                trainId, scheduleId, journeyDate.Date.ToString("yyyyMMdd"));

            var cachedResult = await _cacheService.GetCacheAsync<List<TrainCarDto>>(cacheKey);
            if (cachedResult != null && cachedResult.Any())
            {
                return cachedResult;
            }

            var specification = new TrainIdSpecification(trainId);
            var trainCars = await _unitOfWork.TrainCarRepository.ToListAsync(
                spec: specification,
                orderBy: query => query.OrderByDescending(tc => tc.CarNumber)
            );

            if (!trainCars.Any())
            {
                return new List<TrainCarDto>();
            }

            var result = _mapper.Map<List<TrainCarDto>>(trainCars);

            var schedule = await _unitOfWork.TrainScheduleRepository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                throw new InvalidOperationException($"Schedule with ID {scheduleId} not found");
            }

            var basePrice = CalculateBasePrice(schedule.Distance);
            var daysUntilDeparture = (journeyDate - DateTime.Now).TotalDays;
            var priceMultiplier = daysUntilDeparture <= 7 ? 1.5m : 1m;

            foreach (var trainCarDto in result)
            {
                var (fromPrice, toPrice) = CalculateTrainCarPrices(trainCarDto.SeatType, basePrice, priceMultiplier);
                trainCarDto.FromPrice = fromPrice;
                trainCarDto.ToPrice = toPrice;
            }

            await _cacheService.SetCacheAsync(cacheKey, result, TimeSpan.FromHours(CACHE_HOURS));

            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private static (decimal FromPrice, decimal ToPrice) CalculateTrainCarPrices(
        SeatType seatType,
        decimal basePrice,
        decimal priceMultiplier)
    {
        bool isBusinessClass = seatType == SeatType.Business;
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

    private static decimal CalculateBasePrice(int distance)
    {
        return distance switch
        {
            < 10 => distance * 2000m,
            <= 25 => distance * 1700m,
            <= 50 => distance * 1500m,
            <= 100 => distance * 1300m,
            <= 500 => distance * 1200m,
            _ => distance * 1000m
        };
    }

    public Task<List<TrainCarDto>> GetTrainCarsByTrainIdAsync(Guid trainId)
    {
        var specification = new TrainIdSpecification(trainId);
        return _unitOfWork.TrainCarRepository.ToListAsync<TrainCarDto>(spec: specification);
    }
}
