using Common.Domain;

namespace Admin.Application.Dtos
{
    public class StationDto : BaseEntity
    {
        public string StationName { get; set; }
        public string CityName { get; set; }
        public int KilometricPoint { get; set; }
        public string Location { get; set; }
        public int StationOrder { get; set; }
        public string Coordinates { get; set; }
        public ICollection<TrainStatusDto>? TrainAtStation { get; set; }
    }
}
