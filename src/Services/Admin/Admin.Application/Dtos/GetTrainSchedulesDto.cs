namespace Admin.Application.Dtos;

public class GetTrainSchedulesDto
{
    public Guid DepartureStationId { get; set; }
    public Guid ArrivalStationId { get; set; }
    public DateTime DepartureDate { get; set; }
    public DateTime? ReturnDate { get; set; }
}
