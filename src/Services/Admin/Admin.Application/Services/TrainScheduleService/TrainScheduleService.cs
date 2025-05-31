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

    public TrainScheduleService(
        ITrainScheduleRepository repository,
        IAdminUnitOfWork unitOfWork,
        IMapper mapper,
        IPaginationService paginationService
        ) : base(repository, unitOfWork, mapper, paginationService)
    {
        _adminUnitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<List<TrainScheduleDto>> GetTrainSchedulesAsync(GetTrainSchedulesDto request)
    {
        var specification = new AndSpecification<TrainSchedule>(
            new DepartureStationIdSpecification(request.DepartureStationId),
            new ArrivalStationIdSpecification(request.ArrivalStationId)
        );

        var includes = new List<Expression<Func<TrainSchedule, object>>>
        {
            schedule => schedule.DepartureStation!,
            schedule => schedule.ArrivalStation!,
            schedule => schedule.Train!
        };

        var schedules = await _adminUnitOfWork.TrainScheduleRepository
            .ToListAsync(spec: specification, includes: includes);

        if (!schedules.Any())
            return new List<TrainScheduleDto>();

        var pricingContext = CalculatePricingContext(request.DepartureDate, request.ReturnDate);

        var trainScheduleDtos = _mapper.Map<List<TrainScheduleDto>>(schedules);

        ApplyPricingToSchedules(trainScheduleDtos, schedules, pricingContext);

        return trainScheduleDtos;
    }

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

    private static void ApplyPricingToSchedules(
        List<TrainScheduleDto> dtos,
        List<TrainSchedule> schedules,
        PricingContext pricingContext)
    {
        for (int i = 0; i < dtos.Count; i++)
        {
            var dto = dtos[i];
            var schedule = schedules[i];

            var basePrice = CalculateBasePrice(schedule.Distance);
            var economyPrice = basePrice * pricingContext.PriceMultiplier;
            var businessPrice = economyPrice * 1.3m;

            dto.FromPrice = Math.Round(economyPrice, 2);
            dto.ToPrice = Math.Round(businessPrice, 2);
            dto.Duration = CalculateDurationInMinutes(schedule.DepartureTime, schedule.ArrivalTime);
        }
    }

    private static int CalculateDurationInMinutes(DateTime departureTime, DateTime arrivalTime)
    {
        var duration = arrivalTime - departureTime;
        return (int)duration.TotalMinutes;
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

    public async Task<TrainScheduleDto> GetTrainScheduleInformationAsync(Guid scheduleId)
    {
        var specification = new TrainScheduleIdSpecification(scheduleId);
        var includes = new List<Expression<Func<TrainSchedule, object>>>
        {
            schedule => schedule.DepartureStation!,
            schedule => schedule.ArrivalStation!,
        };
        var schedule = await _adminUnitOfWork.TrainScheduleRepository.FirstOrDefaultAsync<TrainScheduleDto>(spec: specification, includes: includes);
        return schedule;
    }

    private readonly record struct PricingContext(decimal PriceMultiplier);
}
