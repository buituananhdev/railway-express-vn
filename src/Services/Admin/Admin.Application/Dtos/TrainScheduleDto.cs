namespace Admin.Application.Dtos
{
    public class TrainScheduleDto
    {
        public StationDto DepartureStation { get; set; }
        public StationDto ArrivalStation { get; set; }
        public DateTime DepartureTime { get; set; }
        public DateTime ArrivalTime { get; set; }
    }
}
