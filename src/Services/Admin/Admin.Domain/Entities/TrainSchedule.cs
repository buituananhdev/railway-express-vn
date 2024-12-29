using Common.Domain;

namespace Admin.Domain.Entities
{
    public class TrainSchedule : BaseEntity
    {
        public Guid TrainId { get; set; }
        public Train Train { get; set; }
        public Guid DepartureStationId { get; set; }
        public Station DepartureStation { get; set; }
        public Guid ArrivalStationId { get; set; }
        public Station ArrivalStation { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
        public bool IsReturnTrip { get; set; }
    }
}
