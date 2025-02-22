using Admin.Domain.Enums;
using Common.Application.Dtos;
using Common.Domain;

namespace Admin.Application.Dtos;
public class TrainDto : BaseDto
{
    public string TrainName { get; set; }
    public Track Track { get; set; }
    public ICollection<TrainCarDto>? TrainCars { get; set; }
    public TrainStatusDto? Status { get; set; }
}
