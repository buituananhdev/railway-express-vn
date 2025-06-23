using System.Linq.Expressions;
using Admin.Application.Dtos;
using Admin.Application.Repositories;
using Admin.Domain.Entities;
using Admin.Domain.Specifications;
using Admin.Domain.Specifications.TrainSchedule;
using AutoMapper;
using Common.Application.Interfaces;
using Common.Application.Services;
using Common.Domain.Specifications;

namespace Admin.Application.Services;

public class TrainScheduleService : BaseService<TrainSchedule, AddTrainScheduleDto, AddTrainScheduleDto, TrainScheduleDto>, ITrainScheduleService
{
    private readonly IAdminUnitOfWork _adminUnitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;

    private const string TRAIN_SCHEDULES_CACHE_KEY = "train_schedules";
    private const string TRAIN_SCHEDULE_INFO_CACHE_KEY = "train_schedule_info";
    private const string TRAIN_SCHEDULE_CLOSEST_TIME_CACHE_KEY = "train_schedule_closest_time";
    private static readonly TimeSpan CacheDuration = TimeSpan.FromHours(24);
    private static readonly TimeSpan InfoCacheDuration = TimeSpan.FromHours(1);

    private static readonly List<Expression<Func<TrainSchedule, object>>> DefaultIncludes = new List<Expression<Func<TrainSchedule, object>>>
    {
        schedule => schedule.DepartureStation!,
        schedule => schedule.ArrivalStation!,
        schedule => schedule.Train!
    };

    private static readonly List<Expression<Func<TrainSchedule, object>>> InfoIncludes = new List<Expression<Func<TrainSchedule, object>>>
    {
        schedule => schedule.DepartureStation!,
        schedule => schedule.ArrivalStation!
    };

    public TrainScheduleService(
        ITrainScheduleRepository repository,
        IAdminUnitOfWork unitOfWork,
        IMapper mapper,
        IPaginationService paginationService,
        ICacheService cacheService
        ) : base(repository, unitOfWork, mapper, paginationService)
    {
        _adminUnitOfWork = unitOfWork;
        _mapper = mapper;
        _cacheService = cacheService;
    }

    public async Task<List<TrainScheduleDto>> GetTrainSchedulesAsync(GetTrainSchedulesDto request)
    {
        var cacheKey = GenerateSchedulesCacheKey(request);

        var cachedResult = await _cacheService.GetCacheAsync<List<TrainScheduleDto>>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var schedules = await FetchSchedulesFromDatabase(request);

        if (!schedules.Any())
        {
            var emptyResult = new List<TrainScheduleDto>();
            await _cacheService.SetCacheAsync(cacheKey, emptyResult, TimeSpan.FromMinutes(5));
            return emptyResult;
        }

        var result = ProcessSchedules(schedules, request);

        await _cacheService.SetCacheAsync(cacheKey, result, CacheDuration);

        return result;
    }

    public async Task<TrainScheduleDto?> GetTrainScheduleClosestTimeAsync(GetTrainSchedulesDto request, TimeSpan target)
    {
        var cacheKey = GenerateClosestTimeCacheKey(request, target);

        var cachedResult = await _cacheService.GetCacheAsync<TrainScheduleDto>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var schedules = await FetchSchedulesFromDatabase(request);

        if (!schedules.Any())
        {
            await _cacheService.SetCacheAsync(cacheKey, (TrainScheduleDto?)null, TimeSpan.FromMinutes(5));
            return null;
        }

        var closestSchedule = FindClosestScheduleByTime(schedules, target);

        if (closestSchedule == null)
        {
            await _cacheService.SetCacheAsync(cacheKey, (TrainScheduleDto?)null, TimeSpan.FromMinutes(5));
            return null;
        }

        var pricingContext = CalculatePricingContext(request.DepartureTime, request.ReturnTime);
        var result = MapScheduleToDto(closestSchedule, pricingContext);

        await _cacheService.SetCacheAsync(cacheKey, result, CacheDuration);

        return result;
    }

    private static TrainSchedule? FindClosestScheduleByTime(List<TrainSchedule> schedules, TimeSpan target)
    {
        return schedules
            .OrderBy(schedule => Math.Abs((schedule.DepartureTime.TimeOfDay - target).TotalMinutes))
            .FirstOrDefault();
    }

    private static string GenerateClosestTimeCacheKey(GetTrainSchedulesDto request, TimeSpan target)
    {
        var key = $"{TRAIN_SCHEDULE_CLOSEST_TIME_CACHE_KEY}:{request.DepartureStationId}:{request.ArrivalStationId}:{request.DepartureTime:yyyyMMdd}:{target:hhmm}";

        if (request.ReturnTime.HasValue)
        {
            key += $":{request.ReturnTime.Value:yyyyMMdd}";
        }

        return key;
    }

    private async Task<List<TrainSchedule>> FetchSchedulesFromDatabase(GetTrainSchedulesDto request)
    {
        var specifications = new List<Specification<TrainSchedule>> {
                new DepartureStationIdSpecification(request.DepartureStationId),
                new ArrivalStationIdSpecification(request.ArrivalStationId),
        };

        if (request.DepartureTime.Date == DateTime.UtcNow.Date)
        {
            specifications.Add(new DepartureTimeSpecification());
        }

        var andSpecs = new AndSpecificationMultiple<TrainSchedule>(
            specifications
        );

        return await _adminUnitOfWork.TrainScheduleRepository
            .ToListAsync(spec: andSpecs, includes: DefaultIncludes);
    }

    private List<TrainScheduleDto> ProcessSchedules(
        List<TrainSchedule> schedules,
        GetTrainSchedulesDto request)
    {
        var pricingContext = CalculatePricingContext(request.DepartureTime, request.ReturnTime);
        return schedules.Select(schedule => MapScheduleToDto(schedule, pricingContext)).ToList();
    }

    private TrainScheduleDto MapScheduleToDto(TrainSchedule schedule, PricingContext pricingContext)
    {
        var dto = _mapper.Map<TrainScheduleDto>(schedule);
        ApplyPricingToSchedule(dto, schedule, pricingContext);
        return dto;
    }

    private static void ApplyPricingToSchedule(
        TrainScheduleDto dto,
        TrainSchedule schedule,
        PricingContext pricingContext)
    {
        var basePrice = CalculateBasePrice(schedule.Distance);
        var economyPrice = basePrice * pricingContext.PriceMultiplier;
        var businessPrice = economyPrice * 1.3m;

        dto.FromPrice = Math.Round(economyPrice, 2);
        dto.ToPrice = Math.Round(businessPrice, 2);
        dto.Duration = CalculateDurationInMinutes(schedule.DepartureTime, schedule.ArrivalTime);
    }

    private static string GenerateSchedulesCacheKey(GetTrainSchedulesDto request)
    {
        var key = $"{TRAIN_SCHEDULES_CACHE_KEY}:{request.DepartureStationId}:{request.ArrivalStationId}:{request.DepartureTime:yyyyMMdd}";

        if (request.ReturnTime.HasValue)
        {
            key += $":{request.ReturnTime.Value:yyyyMMdd}";
        }

        return key;
    }

    public async Task<TrainScheduleDto> GetTrainScheduleInformationAsync(Guid scheduleId)
    {
        var cacheKey = $"{TRAIN_SCHEDULE_INFO_CACHE_KEY}:{scheduleId}";

        var cachedResult = await _cacheService.GetCacheAsync<TrainScheduleDto>(cacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var specification = new TrainScheduleIdSpecification(scheduleId);
        var schedule = await _adminUnitOfWork.TrainScheduleRepository
            .FirstOrDefaultAsync<TrainScheduleDto>(spec: specification, includes: InfoIncludes);

        if (schedule != null)
        {
            await _cacheService.SetCacheAsync(cacheKey, schedule, InfoCacheDuration);
        }

        return schedule;
    }

    private static decimal CalculateBasePrice(int distance) => distance switch
    {
        < 10 => distance * 2000m,
        <= 25 => distance * 1700m,
        <= 50 => distance * 1500m,
        <= 100 => distance * 1300m,
        <= 500 => distance * 1200m,
        _ => distance * 1000m
    };

    private static PricingContext CalculatePricingContext(DateTime departureDate, DateTime? returnDate)
    {
        var daysUntilDeparture = (departureDate - DateTime.Now).TotalDays;
        var priceMultiplier = 1m;

        if (daysUntilDeparture <= 7)
            priceMultiplier += 0.5m;

        if (returnDate.HasValue)
            priceMultiplier -= 0.2m;

        return new PricingContext(priceMultiplier);
    }

    private static int CalculateDurationInMinutes(DateTime departureTime, DateTime arrivalTime)
    {
        return (int)(arrivalTime - departureTime).TotalMinutes;
    }

    private readonly record struct PricingContext(decimal PriceMultiplier);
}
