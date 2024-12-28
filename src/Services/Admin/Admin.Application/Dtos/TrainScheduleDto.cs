using Common.Domain;

namespace Admin.Application.Dtos
{
    public class TrainScheduleDto : BaseEntity
    {
        public TrainDto Train { get; set; }
        public Guid DepartureStationId { get; set; }
        public Guid ArrivalStationId { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
        public int Duration { get; set; }
        public Decimal Price { get; set; }
    }
}
