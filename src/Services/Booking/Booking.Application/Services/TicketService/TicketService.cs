using AutoMapper;
using Booking.Application.Dtos;
using Booking.Application.Repositories;
using Booking.Domain.Entities;
using Booking.Domain.Specifications;
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

    public async Task<bool> IsSeatBookedForSchedule(Guid seatId, Guid scheduleId, DateTime journeyDate)
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

    public async Task<Dictionary<Guid, bool>> AreSeatBookedForSchedule(List<Guid> seatIds, Guid scheduleId, DateTime journeyDate)
    {
        try
        {
            var baseSpecification = new AndSpecificationMultiple<Ticket>(
                new List<Specification<Ticket>>
                {
                new TicketScheduleIdSpecification(scheduleId),
                new TicketJourneyDateSpecification(journeyDate)
                }
            );

            var bookedTickets = await _bookingUnitOfWork.TicketRepository
                .ToListAsync(spec: baseSpecification);

            var bookedSeatIds = bookedTickets
                .Select(t => t.SeatId)
                .ToHashSet();

            var result = seatIds.ToDictionary(
                seatId => seatId,
                seatId => bookedSeatIds.Contains(seatId)
            );

            return result;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
