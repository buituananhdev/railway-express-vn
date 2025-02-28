using Admin.Domain.Enums;
using Common.Domain;

namespace Admin.Domain.Entities;
public class TrainCar : BaseEntity
{
    public int? CarNumber { get; set; }
    public string? Description { get; set; }
    public SeatType SeatType { get; set; }
    public int TotalSeats { get; set; }
    public Guid TrainId { get; set; }
    public Train Train { get; set; }
    public ICollection<Seat>? Seats { get; set; }
}
