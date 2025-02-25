namespace Admin.Application.Dtos;
public class LockSeatDto
{
    public List<Guid> SeatIds { get; set; }
    public Guid ScheduleId { get; set; }
    public DateTime JourneyDate { get; set; }
}
