using Common.Domain;

namespace Admin.Domain.Entities
{
    public class Station : BaseEntity
    {
        public string StationName { get; set; }
        public string CityName { get; set; }
        public int KilometricPoint { get; set; }
        public string Location { get; set; }
        public int StationOrder { get; set; }
        public string Coordinates { get; set; }
        public ICollection<TrainStatus>? TrainAtStation { get; set; }
        public ICollection<TrainSchedule>? DepartureTrainSchedules { get; set; }
        public ICollection<TrainSchedule>? ArrivalTrainSchedules { get; set; }
    }
}
