using Booking.Domain.Entities;
using Booking.Domain.Enums;

namespace Booking.Application.Dtos;
public class AddTicketDto
{
    public Guid? PassengerId { get; set; }
    public Guid TrainId { get; set; }
    public List<Guid>? SeatIds { get; set; }
    public Guid TrainScheduleId { get; set; }
    public DateTime JourneyDate { get; set; }
}
