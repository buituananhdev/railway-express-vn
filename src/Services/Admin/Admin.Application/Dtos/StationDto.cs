using Common.Domain;

namespace Admin.Application.Dtos
{
    public class StationDto : BaseEntity
    {
        public string StationName { get; set; }
        public string Location { get; set; }
        public int StationOrder { get; set; }
        public ICollection<TrainStatusDto>? TrainAtStation { get; set; }
    }
}
