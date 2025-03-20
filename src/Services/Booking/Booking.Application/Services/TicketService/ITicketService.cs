using System.Linq.Expressions;
using Booking.Application.Dtos;
using Booking.Domain.Entities;
using Common.Application.Dtos;
using Common.Domain.Specifications;

namespace Booking.Application.Services;
public interface ITicketService
{
    Task<TicketDto> CreateAsync(AddTicketDto createDto);
    Task<PaginationResult<TicketDto>> GetListAsync(
        PaginationParams paginationParams,
        Specification<Ticket>? specification = null,
        List<Expression<Func<Ticket, object>>>? includes = null);
    Task<TicketDto> UpdateAsync(Guid id, AddTicketDto updateDto);
    Task DeleteAsync(Guid id);
    Task<TicketDto> GetByIdAsync(Guid id);
    Task<bool> IsSeatBookedForScheduleAsync(Guid seatId, Guid scheduleId, DateTime journeyDate);
    Task<Dictionary<Guid, bool>> AreSeatsBookedForScheduleAsync(List<Guid> seatIds, Guid scheduleId, DateTime journeyDate);
}
