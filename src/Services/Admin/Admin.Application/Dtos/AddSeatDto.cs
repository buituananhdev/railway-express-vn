using Admin.Domain.Enums;

namespace Admin.Application.Dtos;
public class AddSeatDto
{
    public Guid TrainCarId { get; set; }
    public int SeatNumber { get; set; }
    public SeatStatusEnum Status { get; set; }
}
