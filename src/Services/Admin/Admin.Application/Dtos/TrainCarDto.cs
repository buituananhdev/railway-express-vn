using Admin.Domain.Enums;
using Common.Application.Dtos;

namespace Admin.Application.Dtos;
public class TrainCarDto : BaseDto
{
    public int CarNumber { get; set; }
    public string? Description { get; set; }
    public SeatType SeatType { get; set; }
    public int TotalSeats { get; set; }
    public decimal FromPrice { get; set; }
    public decimal ToPrice { get; set; }
}
