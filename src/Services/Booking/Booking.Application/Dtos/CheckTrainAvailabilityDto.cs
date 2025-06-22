namespace Booking.Application.Dtos;
public class CheckTrainAvailabilityDto
{
    public string DepartureStation { get; set; }
    public string ArrivalStation { get; set; }
    public DateTime Date { get; set; }
    public TimeSpan Time { get; set; }
    public int Quantity { get; set; }
}
