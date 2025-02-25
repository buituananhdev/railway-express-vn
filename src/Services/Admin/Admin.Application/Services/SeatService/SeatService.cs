using Admin.Application.Dtos;
using Admin.Application.Repositories;
using Admin.Domain.Specifications;
using AutoMapper;
using Common.Application.Interfaces;
using Common.Protos;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace Admin.Application.Services;

public class SeatService : ISeatService
{
    private readonly BookingGrpcService.BookingGrpcServiceClient _bookingGrpcServiceClient;
    private readonly IAdminUnitOfWork _adminUnitOfWork;
    private readonly IMapper _mapper;
    private readonly ICacheService _cacheService;
    private const int LOCK_DURATION_MINUTES = 5;
    private const string LOCK_PREFIX = "lock:";
    private const string SEATS_CACHE_KEY = "seats:traincar:{0}";
    private const int SEATS_CACHE_MINUTES = 30;

    public SeatService(
        IAdminUnitOfWork adminUnitOfWork,
        IMapper mapper,
        BookingGrpcService.BookingGrpcServiceClient bookingGrpcServiceClient,
        ICacheService cacheService)
    {
        _adminUnitOfWork = adminUnitOfWork;
        _mapper = mapper;
        _bookingGrpcServiceClient = bookingGrpcServiceClient;
        _cacheService = cacheService;
    }

    public async Task<List<SeatDto>> GetSeatsByTrainCarAndScheduleAsync(
    Guid trainCarId,
    Guid trainScheduleId,
    DateTime journeyDate)
    {
        var seatDtos = await GetSeatsFromCacheOrDatabaseAsync(trainCarId);
        if (!seatDtos.Any())
        {
            return seatDtos;
        }

        var bookingStatusTask = GetBookingStatusAsync(seatDtos, trainScheduleId, journeyDate);
        var lockStatusTask = GetLockStatusAsync(seatDtos, trainScheduleId, journeyDate);

        var bookingStatus = await bookingStatusTask;
        var lockStatus = await lockStatusTask;

        for (int i = 0; i < seatDtos.Count; i++)
        {
            string seatIdStr = seatDtos[i].Id.ToString();
            var isBooked = bookingStatus.TryGetValue(seatIdStr, out var booked) && booked;
            var isLocked = lockStatus[i];

            seatDtos[i].Status = isBooked || isLocked
                ? Domain.Enums.SeatStatusEnum.Booked
                : Domain.Enums.SeatStatusEnum.Available;
        }

        return seatDtos;
    }

    private async Task<List<SeatDto>> GetSeatsFromCacheOrDatabaseAsync(Guid trainCarId)
    {
        string cacheKey = string.Format(SEATS_CACHE_KEY, trainCarId);

        var cachedData = await _cacheService.GetCacheAsync<List<SeatDto>>(cacheKey);
        if (cachedData is not null)
        {
            return cachedData;
        }

        var specification = new SeatTrainCarIdSpecification(trainCarId);
        var seats = await _adminUnitOfWork.SeatRepository.ToListAsync(spec: specification);
        var seatDtos = _mapper.Map<List<SeatDto>>(seats);

        if (seatDtos.Any())
        {
            await _cacheService.SetCacheAsync(cacheKey, seatDtos, TimeSpan.FromMinutes(SEATS_CACHE_MINUTES));
        }

        return seatDtos;
    }

    private async Task<MapField<string, bool>> GetBookingStatusAsync(
        List<SeatDto> seats,
        Guid trainScheduleId,
        DateTime journeyDate)
    {
        var request = new BatchCheckSeatStatusRequest
        {
            ScheduleId = trainScheduleId.ToString(),
            JourneyDate = Timestamp.FromDateTime(journeyDate.ToUniversalTime()),
            SeatIds = { seats.Select(s => s.Id.ToString()) }
        };

        var result = await _bookingGrpcServiceClient.BatchCheckSeatStatusAsync(request);
        return result.SeatStatuses;
    }

    private async Task<bool[]> GetLockStatusAsync(
    List<SeatDto> seats,
    Guid scheduleId,
    DateTime journeyDate)
    {
        var lockKeys = seats.Select(seat => GetLockKey(scheduleId, journeyDate, seat.Id)).ToArray();
        return await _cacheService.ExistsMultipleAsync(lockKeys);
    }

    public async Task<bool> IsSeatLocked(Guid seatId, Guid scheduleId, DateTime journeyDate)
    {
        string key = GetLockKey(scheduleId, journeyDate, seatId);
        return await _cacheService.ExistsAsync(key);
    }

    public async Task LockSeatsAsync(LockSeatDto lockSeatDto)
    {
        var lockEntries = lockSeatDto.SeatIds.Select(seatId => new KeyValuePair<string, string>(
                GetLockKey(lockSeatDto.ScheduleId, lockSeatDto.JourneyDate, seatId),
                "locked"
            )).ToList();

        await _cacheService.SetMultipleAsync(lockEntries, TimeSpan.FromMinutes(LOCK_DURATION_MINUTES));
        return;
    }

    private static string GetLockKey(Guid scheduleId, DateTime journeyDate, Guid seatId)
    {
        return $"{LOCK_PREFIX}{scheduleId}:{journeyDate.Date.ToString("yyyyMMdd")}:{seatId}";
    }
}
