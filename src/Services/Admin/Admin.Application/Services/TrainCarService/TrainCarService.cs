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
    private readonly ICacheService _cacheService;

    private const string PRICE_CACHE_KEY = "price:traincar:{0}:schedule:{1}:date:{2}";
    private const int PRICE_CACHE_MINUTES = 10;

    public TrainCarService(
        ITrainCarRepository repository,
        IAdminUnitOfWork unitOfWork,
        IMapper mapper,
        IPaginationService paginationService,
        ISeatService seatService,
        ICacheService cacheService
        ) : base(repository, unitOfWork, mapper, paginationService)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
        _seatService = seatService;
        _cacheService = cacheService;
    }

    public async Task<List<TrainCarDto>> GetTrainCarsAndPriceAsync(Guid trainId, Guid scheduleId, DateTime journeyDate)
    {
        try
        {
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

            var trainCarIds = result.Select(tc => tc.Id).ToList();

            var availableSeatsDict = await _seatService.GetAvailableSeatsForMultipleTrainCarsAsync(
                trainCarIds, scheduleId, journeyDate);

            var pricingTasks = new List<Task>();
            foreach (var trainCarDto in result)
            {
                trainCarDto.AvailableSeats = availableSeatsDict.TryGetValue(trainCarDto.Id, out var availableSeats)
                    ? availableSeats : 0;

                pricingTasks.Add(SetTrainCarPricingAsync(trainCarDto, scheduleId, journeyDate, basePrice, priceMultiplier));
            }

            await Task.WhenAll(pricingTasks);

            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }

    private async Task SetTrainCarPricingAsync(
        TrainCarDto trainCarDto,
        Guid scheduleId,
        DateTime journeyDate,
        decimal basePrice,
        decimal priceMultiplier)
    {
        string cacheKey = string.Format(PRICE_CACHE_KEY,
            trainCarDto.Id, scheduleId, journeyDate.Date.ToString("yyyyMMdd"));

        var cachedPricing = await _cacheService.GetCacheAsync<TrainCarPricingDto>(cacheKey);

        if (cachedPricing != null)
        {
            trainCarDto.FromPrice = cachedPricing.FromPrice;
            trainCarDto.ToPrice = cachedPricing.ToPrice;
            return;
        }

        var (fromPrice, toPrice) = CalculateTrainCarPrices(trainCarDto.SeatType, basePrice, priceMultiplier);

        trainCarDto.FromPrice = fromPrice;
        trainCarDto.ToPrice = toPrice;

        var pricingDto = new TrainCarPricingDto
        {
            FromPrice = fromPrice,
            ToPrice = toPrice
        };
        await _cacheService.SetCacheAsync(cacheKey, pricingDto, TimeSpan.FromMinutes(PRICE_CACHE_MINUTES));
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

public class TrainCarPricingDto
{
    public decimal FromPrice { get; set; }
    public decimal ToPrice { get; set; }
}
