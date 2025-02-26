using Admin.Domain.Enums;
using Common.Application.Dtos;

namespace Admin.Application.Dtos;
public class TrainCarDto : BaseDto
{
    public string? CarNumber { get; set; }
    public string? Description { get; set; }
    public SeatType SeatType { get; set; }
    public int TotalSeats { get; set; }
    public int AvailableSeats { get; set; }
    public decimal FromPrice { get; set; }
    public decimal ToPrice { get; set; }
}
