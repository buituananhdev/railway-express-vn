using System.Linq.Expressions;
using Admin.Application.Dtos;
using Admin.Application.Repositories;
using Admin.Domain.Entities;
using Admin.Domain.Specifications;
using Admin.Domain.Specifications.Seat;
using AutoMapper;
using Common.Application.Dtos;
using Common.Application.Interfaces;
using Common.Application.Pagination;
using Common.Application.Services;
using Common.Domain.Specifications;
using Common.Protos;
using Google.Protobuf.Collections;
using Google.Protobuf.WellKnownTypes;

namespace Admin.Application.Services;

public class SeatService : BaseService<Seat, AddSeatDto, AddSeatDto, SeatDto>, ISeatService
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
    private const string AVAILABLE_SEATS_CACHE_KEY = "available:seats:{0}:{1}:{2}";
    private const int AVAILABLE_SEATS_CACHE_SECONDS = 10;

    public SeatService(
        ISeatRepository repository,
        IAdminUnitOfWork adminUnitOfWork,
        IMapper mapper,
        IPaginationService paginationService,
        BookingGrpcService.BookingGrpcServiceClient bookingGrpcServiceClient,
        ICacheService cacheService) : base(repository, adminUnitOfWork, mapper, paginationService)
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
        string specificStatusCacheKey = $"seats:status:{trainCarId}:{trainScheduleId}:{journeyDate.Date:yyyyMMdd}";

        var cachedResult = await _cacheService.GetCacheAsync<List<SeatDto>>(specificStatusCacheKey);
        if (cachedResult != null)
        {
            return cachedResult;
        }

        var seatDtos = await GetSeatsFromCacheOrDatabaseAsync(trainCarId);
        if (!seatDtos.Any())
        {
            return new List<SeatDto>();
        }

        var result = new List<SeatDto>(seatDtos.Count);

        var seatLookup = seatDtos.ToDictionary(s => s.Id.ToString(), s => s);
        var seatIds = seatDtos.Select(s => s.Id).ToList();

        var bookingStatusTask = GetBookingStatusForSeatsAsync(seatIds, trainScheduleId, journeyDate);
        var lockStatusTask = GetLockStatusForSeatsAsync(seatIds, trainScheduleId, journeyDate);

        await Task.WhenAll(bookingStatusTask, lockStatusTask);

        var bookingStatus = await bookingStatusTask;
        var lockStatus = await lockStatusTask;

        foreach (var seat in seatDtos)
        {
            string seatIdStr = seat.Id.ToString();
            var isBooked = bookingStatus.TryGetValue(seatIdStr, out var booked) && booked;
            var isLocked = lockStatus.TryGetValue(seatIdStr, out var locked) && locked;

            var seatResult = new SeatDto
            {
                Id = seat.Id,
                SeatNumber = seat.SeatNumber,
                TrainCarId = seat.TrainCarId,
                Status = (isBooked || isLocked)
                    ? Domain.Enums.SeatStatusEnum.Booked
                    : Domain.Enums.SeatStatusEnum.Available
            };

            result.Add(seatResult);
        }

        await _cacheService.SetCacheAsync(specificStatusCacheKey, result, TimeSpan.FromSeconds(30));

        return result;
    }

    private async Task<Dictionary<string, bool>> GetBookingStatusForSeatsAsync(
        List<Guid> seatIds,
        Guid trainScheduleId,
        DateTime journeyDate)
    {
        if (!seatIds.Any())
        {
            return new Dictionary<string, bool>();
        }

        var request = new BatchCheckSeatStatusRequest
        {
            ScheduleId = trainScheduleId.ToString(),
            JourneyDate = Timestamp.FromDateTime(journeyDate.ToUniversalTime()),
            SeatIds = { seatIds.Select(s => s.ToString()) }
        };

        var callOptions = new Grpc.Core.CallOptions(deadline: DateTime.UtcNow.AddSeconds(30));
        var result = await _bookingGrpcServiceClient.BatchCheckSeatStatusAsync(request, callOptions);
        return result.SeatStatuses.ToDictionary(kvp => kvp.Key, kvp => kvp.Value);
    }

    private async Task<Dictionary<string, bool>> GetLockStatusForSeatsAsync(
        List<Guid> seatIds,
        Guid trainScheduleId,
        DateTime journeyDate)
    {
        if (!seatIds.Any())
        {
            return new Dictionary<string, bool>();
        }

        var lockKeys = seatIds.Select(seatId => GetLockKey(trainScheduleId, journeyDate, seatId)).ToArray();
        var lockResults = await _cacheService.ExistsMultipleAsync(lockKeys);

        var lockStatus = new Dictionary<string, bool>(seatIds.Count);
        for (int i = 0; i < seatIds.Count; i++)
        {
            lockStatus[seatIds[i].ToString()] = lockResults[i];
        }

        return lockStatus;
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

        var callOptions = new Grpc.Core.CallOptions(deadline: DateTime.UtcNow.AddSeconds(30));
        var result = await _bookingGrpcServiceClient.BatchCheckSeatStatusAsync(request, callOptions);
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
        string availableSeatsCacheKey = string.Format(AVAILABLE_SEATS_CACHE_KEY,
            trainCarId, scheduleId, journeyDate.Date.ToString("yyyyMMdd"));

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

        var seatIds = seatDtos.Select(s => s.Id).ToList();

        var bookingStatusTask = GetBookingStatusForSeatsAsync(seatIds, scheduleId, journeyDate);
        var lockStatusTask = GetLockStatusForSeatsAsync(seatIds, scheduleId, journeyDate);

        await Task.WhenAll(bookingStatusTask, lockStatusTask);

        var bookingStatus = await bookingStatusTask;
        var lockStatus = await lockStatusTask;

        int availableSeats = 0;
        foreach (var seatId in seatIds)
        {
            string seatIdStr = seatId.ToString();
            var isBooked = bookingStatus.TryGetValue(seatIdStr, out var booked) && booked;
            var isLocked = lockStatus.TryGetValue(seatIdStr, out var locked) && locked;

            if (!isBooked && !isLocked)
            {
                availableSeats++;
            }
        }

        await _cacheService.SetCacheAsync(availableSeatsCacheKey, availableSeats, TimeSpan.FromSeconds(AVAILABLE_SEATS_CACHE_SECONDS));
        return availableSeats;
    }

    public async Task<Dictionary<Guid, List<SeatDto>>> GetSeatsByMultipleTrainCarsAndScheduleAsync(
        List<Guid> trainCarIds,
        Guid trainScheduleId,
        DateTime journeyDate)
    {
        if (!trainCarIds.Any())
        {
            return new Dictionary<Guid, List<SeatDto>>();
        }

        var result = new Dictionary<Guid, List<SeatDto>>();
        var uncachedTrainCarIds = new List<Guid>();

        foreach (var trainCarId in trainCarIds)
        {
            string cacheKey = $"seats:status:{trainCarId}:{trainScheduleId}:{journeyDate.Date:yyyyMMdd}";
            var cachedResult = await _cacheService.GetCacheAsync<List<SeatDto>>(cacheKey);

            if (cachedResult != null)
            {
                result[trainCarId] = cachedResult;
            }
            else
            {
                uncachedTrainCarIds.Add(trainCarId);
            }
        }

        if (uncachedTrainCarIds.Any())
        {
            // Get all seats for uncached train cars
            var allSeatsData = new Dictionary<Guid, List<SeatDto>>();
            var allSeatIds = new List<Guid>();

            foreach (var trainCarId in uncachedTrainCarIds)
            {
                var seatDtos = await GetSeatsFromCacheOrDatabaseAsync(trainCarId);
                allSeatsData[trainCarId] = seatDtos;
                allSeatIds.AddRange(seatDtos.Select(s => s.Id));
            }

            if (allSeatIds.Any())
            {
                var bookingStatusTask = GetBookingStatusForSeatsAsync(allSeatIds, trainScheduleId, journeyDate);
                var lockStatusTask = GetLockStatusForSeatsAsync(allSeatIds, trainScheduleId, journeyDate);

                await Task.WhenAll(bookingStatusTask, lockStatusTask);

                var bookingStatus = await bookingStatusTask;
                var lockStatus = await lockStatusTask;

                foreach (var trainCarId in uncachedTrainCarIds)
                {
                    var seatDtos = allSeatsData[trainCarId];
                    var trainCarResult = new List<SeatDto>(seatDtos.Count);

                    foreach (var seat in seatDtos)
                    {
                        string seatIdStr = seat.Id.ToString();
                        var isBooked = bookingStatus.TryGetValue(seatIdStr, out var booked) && booked;
                        var isLocked = lockStatus.TryGetValue(seatIdStr, out var locked) && locked;

                        var seatResult = new SeatDto
                        {
                            Id = seat.Id,
                            SeatNumber = seat.SeatNumber,
                            TrainCarId = seat.TrainCarId,
                            Status = (isBooked || isLocked)
                                ? Domain.Enums.SeatStatusEnum.Booked
                                : Domain.Enums.SeatStatusEnum.Available
                        };

                        trainCarResult.Add(seatResult);
                    }

                    result[trainCarId] = trainCarResult;

                    string cacheKey = $"seats:status:{trainCarId}:{trainScheduleId}:{journeyDate.Date:yyyyMMdd}";
                    await _cacheService.SetCacheAsync(cacheKey, trainCarResult, TimeSpan.FromSeconds(30));
                }
            }
            else
            {
                foreach (var trainCarId in uncachedTrainCarIds)
                {
                    result[trainCarId] = new List<SeatDto>();
                }
            }
        }

        return result;
    }
    public async Task<Dictionary<Guid, int>> GetAvailableSeatsForMultipleTrainCarsAsync(
        List<Guid> trainCarIds,
        Guid scheduleId,
        DateTime journeyDate)
    {
        var result = new Dictionary<Guid, int>();
        var uncachedTrainCarIds = new List<Guid>();

        foreach (var trainCarId in trainCarIds)
        {
            string cacheKey = string.Format(AVAILABLE_SEATS_CACHE_KEY,
                trainCarId, scheduleId, journeyDate.Date.ToString("yyyyMMdd"));

            var cachedCount = await _cacheService.GetCacheAsync<int?>(cacheKey);
            if (cachedCount.HasValue)
            {
                result[trainCarId] = cachedCount.Value;
            }
            else
            {
                uncachedTrainCarIds.Add(trainCarId);
            }
        }

        if (uncachedTrainCarIds.Any())
        {
            var allSeatsData = new Dictionary<Guid, List<SeatDto>>();
            foreach (var trainCarId in uncachedTrainCarIds)
            {
                var seatDtos = await GetSeatsFromCacheOrDatabaseAsync(trainCarId);
                allSeatsData[trainCarId] = seatDtos;
            }

            var allSeats = allSeatsData.Values.SelectMany(seats => seats).ToList();
            if (allSeats.Any())
            {
                var bookingStatus = await GetBookingStatusAsync(allSeats, scheduleId, journeyDate);
                var lockStatus = await GetLockStatusAsync(allSeats, scheduleId, journeyDate);

                var lockStatusLookup = new Dictionary<Guid, bool>();
                for (int i = 0; i < allSeats.Count; i++)
                {
                    lockStatusLookup[allSeats[i].Id] = lockStatus[i];
                }

                foreach (var trainCarId in uncachedTrainCarIds)
                {
                    var seatDtos = allSeatsData[trainCarId];
                    int availableSeats = 0;

                    foreach (var seat in seatDtos)
                    {
                        string seatIdStr = seat.Id.ToString();
                        var isBooked = bookingStatus.TryGetValue(seatIdStr, out var booked) && booked;
                        var isLocked = lockStatusLookup.TryGetValue(seat.Id, out var locked) && locked;

                        if (!isBooked && !isLocked)
                        {
                            availableSeats++;
                        }
                    }

                    result[trainCarId] = availableSeats;

                    string cacheKey = string.Format(AVAILABLE_SEATS_CACHE_KEY,
                        trainCarId, scheduleId, journeyDate.Date.ToString("yyyyMMdd"));
                    await _cacheService.SetCacheAsync(cacheKey, availableSeats, TimeSpan.FromSeconds(AVAILABLE_SEATS_CACHE_SECONDS));
                }
            }
            else
            {
                foreach (var trainCarId in uncachedTrainCarIds)
                {
                    result[trainCarId] = 0;
                }
            }
        }

        return result;
    }

    public async Task<SeatFullInformationDto> GetSeatWithTrainInformationAsync(Guid seatId)
    {
        var specification = new AndSpecificationMultiple<Seat>(
            new List<Specification<Seat>>
            {
                new SeatIdSpecification(seatId)
            }
        );

        var includes = new List<Expression<Func<Seat, object>>>
        {
            t => t.TrainCar,
            s => ((Seat)s).TrainCar.Train
        };

        var seat = await _adminUnitOfWork.SeatRepository
            .FirstOrDefaultAsync(specification, includes);

        return _mapper.Map<SeatFullInformationDto>(seat);
    }

    public async Task<List<Guid>> GetRandomeAvailableSeatAsync(
        Guid trainId,
        Guid trainScheduleId,
        DateTime journeyDate,
        int count)
    {
        var specification = new TrainIdSpecification(trainId);
        var trainCars = await _adminUnitOfWork.TrainCarRepository.ToListAsync<TrainCarDto>(spec: specification);

        var availableSeatIds = new List<Guid>();

        foreach (var trainCar in trainCars)
        {
            var seatDtos = await GetSeatsFromCacheOrDatabaseAsync(trainCar.Id);
            if (!seatDtos.Any())
            {
                continue;
            }

            var bookingStatus = await GetBookingStatusAsync(seatDtos, trainScheduleId, journeyDate);
            var lockStatus = await GetLockStatusAsync(seatDtos, trainScheduleId, journeyDate);

            for (int i = 0; i < seatDtos.Count; i++)
            {
                var seatId = seatDtos[i].Id;
                string seatIdStr = seatId.ToString();
                bool isBooked = bookingStatus.TryGetValue(seatIdStr, out var booked) && booked;
                bool isLocked = lockStatus[i];

                if (!isBooked && !isLocked)
                {
                    availableSeatIds.Add(seatId);
                }
            }
        }

        var random = new Random();
        var shuffled = availableSeatIds.OrderBy(x => random.Next()).Take(count).ToList();

        return shuffled;
    }
}
