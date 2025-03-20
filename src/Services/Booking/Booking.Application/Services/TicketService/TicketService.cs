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

namespace Booking.Application.Services;
public class TicketService : BaseService<Ticket, AddTicketDto, AddTicketDto, TicketDto>, ITicketService
{
    private readonly IBookingUnitOfWork _bookingUnitOfWork;
    private readonly IMapper _mapper;

    public TicketService(ITicketRepository repository,
        IBookingUnitOfWork unitOfWork,
        IMapper mapper,
        IPaginationService paginationService)
        : base(repository, unitOfWork, mapper, paginationService)
    {
        _bookingUnitOfWork = unitOfWork;
        _mapper = mapper;
    }

    public async Task<bool> IsSeatBookedForScheduleAsync(Guid seatId, Guid scheduleId, DateTime journeyDate)
    {
        try
        {
            var specification = new AndSpecificationMultiple<Ticket>(
                new List<Specification<Ticket>>
                {
                new TicketSeatIdSpecification(seatId),
                new TicketScheduleIdSpecification(scheduleId),
                new TicketJourneyDateSpecification(journeyDate)
                }
            );

            return await _bookingUnitOfWork.TicketRepository.ExistsAsync(specification);
        }
        catch (Exception ex)
        {
            throw;
        }
    }

    public async Task<Dictionary<Guid, bool>> AreSeatsBookedForScheduleAsync(List<Guid> seatIds, Guid scheduleId, DateTime journeyDate)
    {
        try
        {
            var specification = new AndSpecificationMultiple<Ticket>(
                new List<Specification<Ticket>>
                {
                    new TicketScheduleIdSpecification(scheduleId),
                    new TicketJourneyDateSpecification(journeyDate),
                    new TicketStatusSpecification(TicketStatusEnum.Active)
                }
            );

            var bookedTickets = await _bookingUnitOfWork.TicketRepository
                .ToListAsync(specification);

            var bookedSeatIds = bookedTickets
                .SelectMany(t => t.SeatIds ?? Enumerable.Empty<Guid>())
                .ToHashSet();

            return seatIds.ToDictionary(id => id, id => bookedSeatIds.Contains(id));
        }
        catch (Exception)
        {
            throw;
        }
    }
}
