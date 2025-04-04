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
    private const int SEATS_CACHE_MINUTES = 60;
    private const string STATUS_CACHE_KEY = "status:schedule:{0}:date:{1}";
    private const int STATUS_CACHE_MINUTES = 1;

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
        string statusCacheKey = string.Format(STATUS_CACHE_KEY, trainScheduleId, journeyDate.Date.ToString("yyyyMMdd"));

        var cachedStatus = await _cacheService.GetCacheAsync<Dictionary<string, bool>>(statusCacheKey);
        Dictionary<string, bool> combinedStatus;

        if (cachedStatus != null)
        {
            combinedStatus = cachedStatus;
        }
        else
        {
            var bookingStatus = await GetBookingStatusAsync(seatDtos, trainScheduleId, journeyDate);

            var lockKeys = seatDtos.Select(seat => GetLockKey(trainScheduleId, journeyDate, seat.Id)).ToArray();
            var lockResults = await _cacheService.ExistsMultipleAsync(lockKeys);

            combinedStatus = new Dictionary<string, bool>();
            for (int i = 0; i < seatDtos.Count; i++)
            {
                string seatIdStr = seatDtos[i].Id.ToString();
                var isBooked = bookingStatus.TryGetValue(seatIdStr, out var booked) && booked;
                var isLocked = lockResults[i];

                combinedStatus[seatIdStr] = isBooked || isLocked;
            }

            await _cacheService.SetCacheAsync(statusCacheKey, combinedStatus, TimeSpan.FromMinutes(STATUS_CACHE_MINUTES));
        }

        var result = new List<SeatDto>(seatDtos.Count);
        foreach (var seat in seatDtos)
        {
            var seatCopy = new SeatDto
            {
                Id = seat.Id,
                SeatNumber = seat.SeatNumber,
                TrainCarId = seat.TrainCarId,
            };

            string seatIdStr = seatCopy.Id.ToString();
            var isUnavailable = combinedStatus.TryGetValue(seatIdStr, out var unavailable) && unavailable;

            seatCopy.Status = isUnavailable
                ? Domain.Enums.SeatStatusEnum.Booked
                : Domain.Enums.SeatStatusEnum.Available;

            result.Add(seatCopy);
        }

        return result;
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

        string statusCacheKey = string.Format(STATUS_CACHE_KEY,
            lockSeatDto.ScheduleId,
            lockSeatDto.JourneyDate.Date.ToString("yyyyMMdd"));

        await _cacheService.RemoveCacheAsync(statusCacheKey);
    }

    private static string GetLockKey(Guid scheduleId, DateTime journeyDate, Guid seatId)
    {
        return $"{LOCK_PREFIX}{scheduleId}:{journeyDate.Date.ToString("yyyyMMdd")}:{seatId}";
    }

    public async Task<int> GetAvailableSeatsAsync(
        Guid trainCarId,
        Guid scheduleId,
        DateTime journeyDate)
    {
        string availableSeatsCacheKey = $"available:seats:{trainCarId}:{scheduleId}:{journeyDate.Date:yyyyMMdd}";
        var cachedCount = await _cacheService.GetCacheAsync<int?>(availableSeatsCacheKey);

        if (cachedCount.HasValue)
        {
            return cachedCount.Value;
        }

        var seatDtos = await GetSeatsFromCacheOrDatabaseAsync(trainCarId);
        if (!seatDtos.Any())
        {
            return 0;
        }

        var bookingStatus = await GetBookingStatusAsync(seatDtos, scheduleId, journeyDate);

        var lockStatus = await GetLockStatusAsync(seatDtos, scheduleId, journeyDate);

        int availableSeats = 0;

        for (int i = 0; i < seatDtos.Count; i++)
        {
            string seatIdStr = seatDtos[i].Id.ToString();
            var isBooked = bookingStatus.TryGetValue(seatIdStr, out var booked) && booked;
            var isLocked = lockStatus[i];

            if (!isBooked && !isLocked)
            {
                availableSeats++;
            }
        }

        await _cacheService.SetCacheAsync(availableSeatsCacheKey, availableSeats, TimeSpan.FromSeconds(30));

        return availableSeats;
    }
}
