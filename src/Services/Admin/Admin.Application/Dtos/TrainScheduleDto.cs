using Common.Application.Dtos;
using Common.Domain;

namespace Admin.Application.Dtos;
public class TrainScheduleDto : BaseDto
{
    public TrainDto Train { get; set; }
    public Guid DepartureStationId { get; set; }
    public Guid ArrivalStationId { get; set; }
    public int Distance { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
    public int Duration { get; set; }
    public Decimal FromPrice { get; set; }
    public Decimal ToPrice { get; set; }

}
