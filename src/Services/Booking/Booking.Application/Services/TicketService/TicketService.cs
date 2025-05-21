using System.Linq.Expressions;
using AutoMapper;
using Booking.Application.Dtos;
using Booking.Application.Repositories;
using Booking.Domain.Entities;
using Booking.Domain.Enums;
using Booking.Domain.Specifications;
using Booking.Domain.Specifications.Ticket;
using Common.Application.Interfaces;
using Common.Application.Services;
using Common.Domain.Specifications;
using Microsoft.Extensions.Logging;

namespace Booking.Application.Services
{
    public class TicketService : BaseService<Ticket, AddTicketDto, AddTicketDto, TicketDto>, ITicketService
    {
        private readonly IBookingUnitOfWork _bookingUnitOfWork;
        private readonly IMapper _mapper;
        private readonly IDistributedLockService _lockService;
        private readonly ICacheService _cacheService;
        private readonly ILogger<TicketService> _logger;
        private const string SEAT_LOCK_KEY_PREFIX = "seat:lock:";
        private const int LOCK_TIMEOUT_SECONDS = 5;

        public TicketService(
            ITicketRepository repository,
            IBookingUnitOfWork unitOfWork,
            IMapper mapper,
            IPaginationService paginationService,
            IDistributedLockService lockService,
            ICacheService cacheService,
            ILogger<TicketService> logger)
            : base(repository, unitOfWork, mapper, paginationService)
        {
            _bookingUnitOfWork = unitOfWork;
            _mapper = mapper;
            _lockService = lockService;
            _cacheService = cacheService;
            _logger = logger;
        }

        public override async Task<TicketDto> CreateAsync(AddTicketDto createDto)
        {
            createDto.TicketNumber = GenerateTicketNumber();
            if (createDto.SeatIds?.Count == 1)
            {
                return await BookSingleSeatAsync(createDto);
            }
            else if (createDto.SeatIds?.Count > 1)
            {
                return await BookMultipleSeatsAsync(createDto);
            }
            else
            {
                throw new ArgumentException("No seat IDs provided for booking");
            }
        }

        private async Task<TicketDto> BookSingleSeatAsync(AddTicketDto createDto)
        {
            var seatId = createDto.SeatIds[0];
            var lockKey = GetSeatLockKey(seatId, createDto.TrainScheduleId, createDto.JourneyDate);

            try
            {
                return await _lockService.ExecuteWithLockAsync<TicketDto>(
                    lockKey,
                    async () =>
                    {
                        bool isBooked = await IsSeatBookedForScheduleAsync(seatId, createDto.TrainScheduleId, createDto.JourneyDate);
                        if (isBooked)
                        {
                            throw new InvalidOperationException($"Seat {seatId} is already booked for this schedule and date.");
                        }

                        var entity = _mapper.Map<Ticket>(createDto);
                        await _bookingUnitOfWork.TicketRepository.AddAsync(entity);
                        await _bookingUnitOfWork.SaveChangesAsync();

                        await CacheSeatBookingInfoAsync(seatId, createDto.TrainScheduleId, createDto.JourneyDate);

                        return _mapper.Map<TicketDto>(entity);
                    },
                    TimeSpan.FromSeconds(LOCK_TIMEOUT_SECONDS)
                );
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Could not acquire lock"))
            {
                _logger.LogWarning("Concurrent booking attempt for seat {SeatId} on schedule {ScheduleId}", seatId, createDto.TrainScheduleId);
                throw new InvalidOperationException("This seat is currently being booked by another user. Please try again.");
            }
        }

        private async Task<TicketDto> BookMultipleSeatsAsync(AddTicketDto createDto)
        {
            // For multiple seats, we need to lock all seats simultaneously
            var lockKeys = createDto.SeatIds.Select(seatId =>
                GetSeatLockKey(seatId, createDto.TrainScheduleId, createDto.JourneyDate)).ToList();

            var mainLockKey = $"multi:seat:lock:{createDto.TrainScheduleId}:{createDto.JourneyDate}:{Guid.NewGuid()}";

            try
            {
                return await _lockService.ExecuteWithLockAsync<TicketDto>(
                    mainLockKey,
                    async () =>
                    {
                        var bookingStatus = await AreSeatsBookedForScheduleAsync(
                            createDto.SeatIds, createDto.TrainScheduleId, createDto.JourneyDate);

                        var bookedSeats = bookingStatus.Where(kv => kv.Value).Select(kv => kv.Key).ToList();
                        if (bookedSeats.Any())
                        {
                            throw new InvalidOperationException($"Seats {string.Join(", ", bookedSeats)} are already booked.");
                        }

                        var entity = _mapper.Map<Ticket>(createDto);
                        await _bookingUnitOfWork.TicketRepository.AddAsync(entity);
                        await _bookingUnitOfWork.SaveChangesAsync();

                        foreach (var seatId in createDto.SeatIds)
                        {
                            await CacheSeatBookingInfoAsync(seatId, createDto.TrainScheduleId, createDto.JourneyDate);
                        }

                        return _mapper.Map<TicketDto>(entity);
                    },
                    TimeSpan.FromSeconds(LOCK_TIMEOUT_SECONDS)
                );
            }
            catch (InvalidOperationException ex) when (ex.Message.Contains("Could not acquire lock"))
            {
                _logger.LogWarning("Concurrent booking attempt for multiple seats on schedule {ScheduleId}", createDto.TrainScheduleId);
                throw new InvalidOperationException("One or more seats are currently being booked by another user. Please try again.");
            }
        }

        private string GetSeatLockKey(Guid seatId, Guid scheduleId, DateTime journeyDate)
        {
            return $"{SEAT_LOCK_KEY_PREFIX}{scheduleId}:{journeyDate:yyyyMMdd}:{seatId}";
        }

        private async Task CacheSeatBookingInfoAsync(Guid seatId, Guid scheduleId, DateTime journeyDate)
        {
            string cacheKey = $"seat:booked:{scheduleId}:{journeyDate:yyyyMMdd}:{seatId}";
            await _cacheService.SetCacheAsync(cacheKey, true, TimeSpan.FromMinutes(10));
        }

        public async Task<bool> IsSeatBookedForScheduleAsync(Guid seatId, Guid scheduleId, DateTime journeyDate)
        {
            try
            {
                string cacheKey = $"seat:booked:{scheduleId}:{journeyDate:yyyyMMdd}:{seatId}";
                var cachedResult = await _cacheService.GetCacheAsync<bool?>(cacheKey);

                if (cachedResult.HasValue)
                {
                    return cachedResult.Value;
                }

                var specification = new AndSpecificationMultiple<Ticket>(
                    new List<Specification<Ticket>>
                    {
                        new TicketSeatIdSpecification(seatId),
                        new TicketScheduleIdSpecification(scheduleId),
                        new TicketJourneyDateSpecification(journeyDate),
                        new TicketStatusSpecification(TicketStatusEnum.Active)
                    }
                );

                bool isBooked = await _bookingUnitOfWork.TicketRepository.ExistsAsync(specification);

                await _cacheService.SetCacheAsync(cacheKey, isBooked, TimeSpan.FromMinutes(1));

                return isBooked;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if seat is booked");
                throw;
            }
        }

        public async Task<Dictionary<Guid, bool>> AreSeatsBookedForScheduleAsync(List<Guid> seatIds, Guid scheduleId, DateTime journeyDate)
        {
            try
            {
                var result = new Dictionary<Guid, bool>();

                var cacheKeys = seatIds.Select(id => $"seat:booked:{scheduleId}:{journeyDate:yyyyMMdd}:{id}").ToArray();
                var cacheExists = await _cacheService.ExistsMultipleAsync(cacheKeys);

                var uncachedSeatIds = new List<Guid>();

                for (int i = 0; i < seatIds.Count; i++)
                {
                    if (cacheExists[i])
                    {
                        var isBooked = await _cacheService.GetCacheAsync<bool>(cacheKeys[i]);
                        result[seatIds[i]] = isBooked;
                    }
                    else
                    {
                        uncachedSeatIds.Add(seatIds[i]);
                    }
                }

                if (uncachedSeatIds.Any())
                {
                    var specification = new AndSpecificationMultiple<Ticket>(
                        new List<Specification<Ticket>>
                        {
                            new TicketScheduleIdSpecification(scheduleId),
                            new TicketJourneyDateSpecification(journeyDate),
                            new TicketStatusSpecification(TicketStatusEnum.Active)
                        }
                    );

                    var bookedTickets = await _bookingUnitOfWork.TicketRepository.ToListAsync(specification);
                    var bookedSeatIds = bookedTickets
                        .SelectMany(t => t.SeatIds ?? Enumerable.Empty<Guid>())
                        .ToHashSet();

                    foreach (var seatId in uncachedSeatIds)
                    {
                        bool isBooked = bookedSeatIds.Contains(seatId);
                        result[seatId] = isBooked;

                        string cacheKey = $"seat:booked:{scheduleId}:{journeyDate:yyyyMMdd}:{seatId}";
                        await _cacheService.SetCacheAsync(cacheKey, isBooked, TimeSpan.FromMinutes(1));
                    }
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking if multiple seats are booked");
                throw;
            }
        }

        public async Task UpdateTicketStatusAsync(Guid ticketId, TicketStatusEnum status)
        {
            try
            {
                var ticket = await _bookingUnitOfWork.TicketRepository.GetByIdAsync(ticketId);
                ticket.Status = status;
                await _bookingUnitOfWork.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        private string GenerateTicketNumber()
        {
            int remainingChars = 7;

            string timestamp = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds().ToString();
            Random random = new Random();
            int randomPart = random.Next(1000);

            string uniqueIdentifier = $"{timestamp}{randomPart:D3}";

            if (uniqueIdentifier.Length > remainingChars)
            {
                uniqueIdentifier = uniqueIdentifier.Substring(uniqueIdentifier.Length - remainingChars);
            }

            string ticketNumber = $"#BO-{uniqueIdentifier}";

            return ticketNumber;
        }

        public async Task<TicketDto> GetTicketWithPassengerInfoAsync(Guid ticketId)
        {
            var specification = new AndSpecificationMultiple<Ticket>(
                new List<Specification<Ticket>>
                {
                    new TicketIdSpecification(ticketId)
                }
            );

            var includes = new List<Expression<Func<Ticket, object>>>
            {
                t => t.PassengerDetails,
            };

            var ticket = await _bookingUnitOfWork.TicketRepository
                .FirstOrDefaultAsync(specification, includes);

            return _mapper.Map<TicketDto>(ticket);
        }

        public Task<TicketDto> CreateTicketForDialogfowAsync(DialogflowCreateTicketRequest request) => throw new NotImplementedException();
    }
}
