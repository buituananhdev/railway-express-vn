using Common.Domain;

namespace Admin.Domain.Entities;
public class TrainStatus : BaseEntity
{
    public Guid TrainId { get; set; }
    public Train Train { get; set; }
    public Guid StationId { get; set; }
    public Station Station { get; set; }
    public Admin.Domain.Enums.TrainStatus Status { get; set; }
    public string Remarks { get; set; }
}
