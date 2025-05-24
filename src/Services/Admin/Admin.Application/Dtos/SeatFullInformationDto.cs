using Admin.Domain.Enums;

namespace Admin.Application.Dtos;
public class SeatFullInformationDto
{
    public TrainCarFullInformationDto TrainCar { get; set; }
    public int SeatNumber { get; set; }
    public SeatStatusEnum Status { get; set; }
}
