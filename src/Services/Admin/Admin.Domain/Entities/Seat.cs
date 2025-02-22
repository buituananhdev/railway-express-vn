using Common.Domain;

namespace Admin.Domain.Entities;
public class Seat : BaseEntity
{
    public Guid TrainCarId { get; set; }
    public TrainCar TrainCar { get; set; }
    public string SeatNumber { get; set; }
}
