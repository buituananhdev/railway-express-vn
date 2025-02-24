using Booking.Domain.Enums;
using Common.Domain;

namespace Booking.Domain.Entities;
public class Ticket : BaseEntity
{
    public Guid PassengerId { get; set; }
    public Guid TrainId { get; set; }
    public Guid SeatId { get; set; }
    public Guid TrainScheduleId { get; set; }
    public DateTime BookingDate { get; set; } = DateTime.Now;
    public DateTime JourneyDate { get; set; }
    public decimal TotalPrice { get; set; }
    public TicketStatusEnum Status { get; set; }
    public string? Remarks { get; set; }
    public ICollection<PassengerInfo> PassengerDetails { get; set; } 
}
