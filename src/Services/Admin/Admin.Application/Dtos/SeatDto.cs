using Admin.Domain.Enums;
using Common.Application.Dtos;

namespace Admin.Application.Dtos;
public class SeatDto : BaseDto
{
    public Guid TrainCarId { get; set; }
    public string SeatNumber { get; set; }
    public SeatStatusEnum Status { get; set; }
}
