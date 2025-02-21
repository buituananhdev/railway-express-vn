using Common.Domain;

namespace Booking.Domain.Entities;
public class Ticket : BaseEntity
{
    public Guid PassengerId { get; set; }
    public Guid TrainId { get; set; }
    public Guid SeatId { get; set; }
    public Guid DepartureStationId { get; set; }
    public Guid ArrivalStationId { get; set; }
    public decimal TotalPrice { get; set; }
}
