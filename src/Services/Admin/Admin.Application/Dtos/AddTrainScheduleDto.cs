namespace Admin.Application.Dtos
{
    public class AddTrainScheduleDto
    {
        public Guid DepartureStationId { get; set; }
        public Guid ArrivalStationId { get; set; }
        public TimeSpan DepartureTime { get; set; }
        public TimeSpan ArrivalTime { get; set; }
    }
}
