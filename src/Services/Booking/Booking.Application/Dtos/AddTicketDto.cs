using Booking.Domain.Enums;

namespace Booking.Application.Dtos;
public class AddTicketDto
{
    public Guid PassengerId { get; set; }
    public Guid TrainId { get; set; }
    public Guid SeatId { get; set; }
    public Guid TrainScheduleId { get; set; }
    public DateTime JourneyDate { get; set; }
    public decimal TotalPrice { get; set; }
    public TicketStatusEnum Status { get; set; }
    public string? Remarks { get; set; }
}
