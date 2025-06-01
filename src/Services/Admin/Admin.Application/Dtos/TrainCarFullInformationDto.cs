using Admin.Domain.Enums;

namespace Admin.Application.Dtos;

public class TrainCarFullInformationDto
{
    public Guid Id { get; set; }
    public int? CarNumber { get; set; }
    public SeatType SeatType { get; set; }
    public TrainFullInformationDto Train { get; set; }
}
