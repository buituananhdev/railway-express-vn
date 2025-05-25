namespace Booking.Application.Dtos;
public class DialogflowCreateTicketRequest
{
    public string DepartureStation { get; set; }
    public string ArrivalStation { get; set; }
    public DateTime Date { get; set; }
    public int Quantity { get; set; }
    public TimeSpan Time { get; set; }
    public string PassengerName { get; set; }
    public string PassengerEmail { get; set; }
}
