namespace Admin.Application.Dtos
{
    public class AddTrainScheduleDto
    {
        public Guid DepartureStationId { get; set; }
        public Guid ArrivalStationId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
    }
}
