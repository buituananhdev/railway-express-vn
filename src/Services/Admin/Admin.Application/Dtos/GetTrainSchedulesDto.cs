namespace Admin.Application.Dtos
{
    public class GetTrainSchedulesDto
    {
        public Guid DepartureStationId { get; set; }
        public Guid ArrivalStationId { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime? ReturnTime { get; set; } = null;
        public int NumberOfPassengers { get; set; }
    }
}
