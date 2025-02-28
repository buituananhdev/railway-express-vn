using Booking.Application.Dtos;

namespace Booking.Application.Services;
public interface ITicketService
{
    Task<Guid> AddTicketAsync(AddTicketDto addTicketDto);
    Task<bool> IsSeatBookedForScheduleAsync(Guid seatId, Guid scheduleId, DateTime journeyDate);
    Task<Dictionary<Guid, bool>> AreSeatsBookedForScheduleAsync(List<Guid> seatIds, Guid scheduleId, DateTime journeyDate);
}
