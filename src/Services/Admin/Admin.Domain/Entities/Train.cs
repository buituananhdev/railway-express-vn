using Admin.Domain.Enums;
using Common.Domain;

namespace Admin.Domain.Entities;
public class Train : BaseEntity
{
    public string TrainName { get; set; }
    public Track Track { get; set; }
    public ICollection<TrainCar>? TrainCars { get; set; }
    public TrainStatus? Status { get; set; }
    public ICollection<TrainSchedule> TrainSchedules { get; set; }
}
