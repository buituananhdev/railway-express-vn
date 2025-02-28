using AutoMapper;
using Booking.Application.Dtos;
using Booking.Application.Repositories;
using Booking.Domain.Entities;
using Booking.Domain.Enums;
using Booking.Domain.Specifications;
using Booking.Domain.Specifications.Ticket;
using Common.Domain.Specifications;

namespace Booking.Application.Services;
public class TicketService : ITicketService
{
    private readonly IBookingUnitOfWork _bookingUnitOfWork;
    private readonly IMapper _mapper;

    public TicketService(IBookingUnitOfWork bookingUnitOfWork, IMapper mapper)
    {
        _bookingUnitOfWork = bookingUnitOfWork;
        _mapper = mapper;
    }
    public async Task<TicketDto> AddTicketAsync(AddTicketDto addTicketDto)
    {
        try
        {
            var passenger = _mapper.Map<Ticket>(addTicketDto);
            await _bookingUnitOfWork.TicketRepository.AddAsync(passenger);
            await _bookingUnitOfWork.SaveChangesAsync();

            return _mapper.Map<TicketDto>(passenger);
        }
        catch (Exception ex)
        {
            throw;
        }
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
