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
using Common.Contracts.Events;
using Common.Domain.Specifications;
using Common.Protos;
using Google.Protobuf.WellKnownTypes;
using MassTransit;
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
        private readonly ITicketSeatService _ticketSeatService;
        private readonly IBookingOrderService _bookingOrderService;
        private readonly IPublishEndpoint _publishEndpoint;
        private readonly AdminGrpcService.AdminGrpcServiceClient _adminGrpcServiceClient;

        private const string SEAT_LOCK_KEY_PREFIX = "seat:lock:";
        private const int LOCK_TIMEOUT_SECONDS = 5;

        public TicketService(
            ITicketRepository repository,
            IBookingUnitOfWork unitOfWork,
            IMapper mapper,
            IPaginationService paginationService,
            IDistributedLockService lockService,
            ICacheService cacheService,
            ILogger<TicketService> logger,
            ITicketSeatService ticketSeatService,
            IBookingOrderService bookingOrderService,
            IPublishEndpoint publishEndpoint,
            AdminGrpcService.AdminGrpcServiceClient adminGrpcServiceClient)
            : base(repository, unitOfWork, mapper, paginationService)
        {
            _bookingUnitOfWork = unitOfWork;
            _mapper = mapper;
            _lockService = lockService;
            _cacheService = cacheService;
            _logger = logger;
            _ticketSeatService = ticketSeatService;
            _adminGrpcServiceClient = adminGrpcServiceClient;
            _bookingOrderService = bookingOrderService;
            _publishEndpoint = publishEndpoint;
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

                        BookingOrderDto bookingOrder;
                        if (createDto.TicketType == TicketTypeEnum.Normal)
                        {
                            bookingOrder = await _bookingOrderService.CreateAsync(new AddBookingOrderDto());
                        }
                        else
                        {
                            bookingOrder = await _bookingOrderService.GetByIdAsync((Guid)createDto.BookingOrderId);
                        }

                        createDto.BookingOrderId = bookingOrder.Id;
                        var entity = _mapper.Map<Ticket>(createDto);
                        await _bookingUnitOfWork.TicketRepository.AddAsync(entity);
                        await _bookingUnitOfWork.SaveChangesAsync();

                        var ticketSeat = new AddTicketSeatDto
                        {
                            TicketId = entity.Id,
                            SeatId = seatId
                        };

                        var ticketSeatDto = await _ticketSeatService.CreateAsync(ticketSeat);

                        await CacheSeatBookingInfoAsync(seatId, createDto.TrainScheduleId, createDto.JourneyDate);

                        var ticket = _mapper.Map<TicketDto>(entity);
                        ticket.TicketSeats = new List<TicketSeatDto> { ticketSeatDto };
                        return ticket;
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

                        BookingOrderDto bookingOrder;
                        if (createDto.TicketType == TicketTypeEnum.Normal)
                        {
                            bookingOrder = await _bookingOrderService.CreateAsync(new AddBookingOrderDto());
                        }
                        else
                        {
                            bookingOrder = await _bookingOrderService.GetByIdAsync((Guid)createDto.BookingOrderId);
                        }

                        createDto.BookingOrderId = bookingOrder.Id;

                        var entity = _mapper.Map<Ticket>(createDto);
                        await _bookingUnitOfWork.TicketRepository.AddAsync(entity);
                        await _bookingUnitOfWork.SaveChangesAsync();

                        var ticketSeatDtos = new List<TicketSeatDto>();
                        foreach (var seatId in createDto.SeatIds)
                        {
                            var ticketSeat = new AddTicketSeatDto
                            {
                                TicketId = entity.Id,
                                SeatId = seatId
                            };

                            var ticketSeatDto = await _ticketSeatService.CreateAsync(ticketSeat);
                            ticketSeatDtos.Add(ticketSeatDto);

                            await CacheSeatBookingInfoAsync(seatId, createDto.TrainScheduleId, createDto.JourneyDate);
                        }

                        var ticket = _mapper.Map<TicketDto>(entity);
                        ticket.TicketSeats = ticketSeatDtos;
                        return ticket;
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

                    var includes = new List<Expression<Func<Ticket, object>>>
                    {
                        t => t.TicketSeats
                    };

                    var bookedTickets = await _bookingUnitOfWork.TicketRepository.ToListAsync(spec: specification, includes: includes);
                    var bookedSeatIds = bookedTickets
                        .SelectMany(t => t.TicketSeats)
                        .Select(ts => ts.SeatId)
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

        public async Task UpdateTicketsStatusAsync(Guid bookingOrderId, TicketStatusEnum status)
        {
            var specification = new BookingOrderIdSpecification(bookingOrderId);
            var tickets = await _bookingUnitOfWork.TicketRepository.ToListForUpdateAsync<Ticket>(spec: specification);
            foreach (var ticket in tickets)
            {
                ticket.Status = status;
            }
            await _bookingUnitOfWork.SaveChangesAsync();
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

        public async Task<List<TicketDto>> GetTicketWithPassengerInfoAsync(Guid bookingOrderId)
        {
            var specification = new BookingOrderIdSpecification(bookingOrderId);

            var tickets = await _bookingUnitOfWork.TicketRepository.ToListAsync<TicketDto>(spec: specification);

            foreach (var ticket in tickets)
            {
                var request = new GetSeatInformationRequest { SeatId = ticket.TicketSeats.FirstOrDefault()?.SeatId.ToString() };
                var seatInformation = await _adminGrpcServiceClient.GetSeatInformationAsync(request);
                ticket.SeatInformations ??= new List<Seat>();
                ticket.SeatInformations.Add(_mapper.Map<Seat>(seatInformation));
            }
            return tickets;
        }

        public Task<TicketDto> GetTicketByTicketNumberAsync(string ticketNumber)
        {
            var specification = new TicketNumberSpecification(ticketNumber);
            return GetTicketAsync(specification);
        }

        public override Task<TicketDto> GetByIdAsync(Guid ticketId)
        {
            var specification = new TicketIdSpecification(ticketId);
            return GetTicketAsync(specification);
        }

        private async Task<TicketDto> GetTicketAsync(Specification<Ticket> specification)
        {
            var includes = new List<Expression<Func<Ticket, object>>>
                {
                    t => t.PassengerDetails,
                    t => t.TicketSeats,
                };

            var ticket = await _bookingUnitOfWork.TicketRepository
                .FirstOrDefaultAsync(specification, includes);
            var ticketDto = _mapper.Map<TicketDto>(ticket);
            foreach (var ticketSeat in ticket.TicketSeats)
            {
                var request = new GetSeatInformationRequest { SeatId = ticketSeat.SeatId.ToString() };
                var seat = await _adminGrpcServiceClient.GetSeatInformationAsync(request);
                ticketDto.SeatInformations ??= new List<Seat>();
                ticketDto.SeatInformations.Add(_mapper.Map<Seat>(seat));
            }
            var schedule = await _adminGrpcServiceClient.GetTrainScheduleInformationAsync(new GetTrainScheduleInformationRequest
            {
                ScheduleId = ticket.TrainScheduleId.ToString()
            });
            ticketDto.TrainSchedule = _mapper.Map<TrainSchedule>(schedule);

            return ticketDto;
        }

        public async Task<TicketDto> CreateTicketForDialogfowAsync(DialogflowCreateTicketRequest request)
        {
            var departureStationId = await _adminGrpcServiceClient.GetStationInformationAsync(new GetStationInformationRequest { StationName = request.DepartureStation });
            var arrivalStation = await _adminGrpcServiceClient.GetStationInformationAsync(new GetStationInformationRequest { StationName = request.ArrivalStation });
            var scheduleRequest = new GetTrainScheduleRequest
            {
                DepartureStationId = departureStationId.StationId,
                ArrivalStationId = arrivalStation.StationId,
                DepartureDate = Timestamp.FromDateTime(request.Date.ToUniversalTime()),
                DepartureTime = Duration.FromTimeSpan(request.Time),
            };
            var schedule = await _adminGrpcServiceClient.GetTrainScheduleAsync(scheduleRequest);

            var seatIds = await _adminGrpcServiceClient.GetRandomeAvailableSeatAsync(new GetRandomeAvailableSeatRequest
            {
                TrainId = schedule.TrainId,
                ScheduleId = schedule.TrainScheduleId,
                JourneyDate = Timestamp.FromDateTime(request.Date.ToUniversalTime()),
                Quantity = request.Quantity
            });

            var createDto = new AddTicketDto
            {
                TicketNumber = GenerateTicketNumber(),
                SeatIds = seatIds.SeatIds.Select(id => Guid.Parse(id)).ToList(),
                TrainId = Guid.Parse(schedule.TrainId),
                JourneyDate = request.Date,
                TrainScheduleId = Guid.Parse(schedule.TrainScheduleId),
                TotalPrice = (decimal)schedule.BasePrice * request.Quantity
            };

            var ticketDto = await CreateAsync(createDto);

            return ticketDto;
        }

        public async Task<double> GetTicketPricesByBookingOrderAsync(Guid bookingOrderId)
        {
            var specification = new BookingOrderIdSpecification(bookingOrderId);

            var tickets = await _bookingUnitOfWork.TicketRepository.ToListAsync(specification);

            return tickets.Sum(t => (double)t.TotalPrice);
        }

        public async Task CancelTicketAsync(Guid ticketId)
        {
            var ticket = await _bookingUnitOfWork.TicketRepository.GetByIdAsync(ticketId);
            ticket.Status = TicketStatusEnum.Cancelled;
            await _bookingUnitOfWork.SaveChangesAsync();
        }

        public async Task SendETicketAsync(Guid bookingOrderId)
        {
            var specification = new BookingOrderIdSpecification(bookingOrderId);
            var includes = new List<Expression<Func<Ticket, object>>>
            {
                t => t.PassengerDetails,
                t => t.TicketSeats,
            };
            var tickets = await _bookingUnitOfWork.TicketRepository.ToListAsync<TicketDto>(spec: specification, includes: includes);
            
            foreach (var ticket in tickets)
            {
                foreach (var ticketSeat in ticket.TicketSeats)
                {
                    var request = new GetSeatInformationRequest { SeatId = ticketSeat.SeatId.ToString() };
                    var seat = await _adminGrpcServiceClient.GetSeatInformationAsync(request);
                    ticket.SeatInformations ??= new List<Seat>();
                    ticket.SeatInformations.Add(_mapper.Map<Seat>(seat));
                }

                var schedule = await _adminGrpcServiceClient.GetTrainScheduleInformationAsync(new GetTrainScheduleInformationRequest
                {
                    ScheduleId = ticket.TrainScheduleId.ToString()
                });

                ticket.TrainSchedule = _mapper.Map<TrainSchedule>(schedule);

                if (ticket.PassengerDetails == null || !ticket.PassengerDetails.Any())
                {
                    continue;
                }

                foreach (var passenger in ticket.PassengerDetails)
                {
                    var passengerSeat = ticket.TicketSeats?.FirstOrDefault(ts => ts.PassengerInfoId == passenger.Id);
                    var seatInfo = ticket.SeatInformations?.FirstOrDefault(s => s.Id == passengerSeat.SeatId.ToString());

                    string ticketTypeName = GetTicketTypeName(ticket.TicketType);

                    var eTicketEvent = new ETicketEvent(
                        TicketNumber: ticket.TicketNumber,
                        PassengerName: $"{passenger.FirstName} {passenger.LastName}",
                        Email: passenger.Email,
                        TicketType: ticketTypeName,
                        Journey: new JourneyInfo(
                            DepartureStation: ticket.TrainSchedule?.DepartureStation?.StationName ?? "Unknown",
                            ArrivalStation: ticket.TrainSchedule?.ArrivalStation?.StationName ?? "Unknown",
                            DepartureDate: ticket.TrainSchedule?.DepartureTime.Date ?? ticket.JourneyDate.Date,
                            ArrivalDate: ticket.TrainSchedule?.ArrivalTime.Date ?? ticket.JourneyDate.Date,
                            DepartureTime: ticket.TrainSchedule?.DepartureTime.TimeOfDay ?? TimeSpan.Zero,
                            ArrivalTime: ticket.TrainSchedule?.ArrivalTime.TimeOfDay ?? TimeSpan.Zero,
                            TrainNumber: seatInfo?.TrainCar?.Train?.TrainName ?? "Unknown",
                            CarriageNumber: seatInfo?.TrainCar?.CarNumber?.ToString() ?? "Unknown",
                            SeatNumber: seatInfo?.SeatNumber.ToString() ?? "Unknown"
                        ),
                        BookingDate: ticket.BookingDate,
                        QrCodeUrl: "https://res.cloudinary.com/ddqjbrc8q/image/upload/fl_preserve_transparency/v1748769468/website_qrcode_x2rv6u.jpg?_s=public-apps",
                        LogoUrl: "https://res.cloudinary.com/ddqjbrc8q/image/upload/fl_preserve_transparency/v1748230281/logo_mfewmk.jpg?_s=public-apps"
                    );

                    await _publishEndpoint.Publish(eTicketEvent);
                }
            }
        }

        private string GetTicketTypeName(TicketTypeEnum ticketType)
        {
            return ticketType switch
            {
                TicketTypeEnum.Normal => "Vé thường",
                TicketTypeEnum.Return => "Vé khứ hồi",
            };
        }
    }
}
