using Admin.Domain.Enums;

namespace Admin.Application.Dtos;
public class AddTrainCarDto
{
    public int CarNumber { get; set; }
    public string? Description { get; set; }
    public SeatType SeatType { get; set; }
    public int TotalSeats { get; set; }
    public Guid TrainId { get; set; }
}
