using Admin.Domain.Enums;
using Common.Domain;

namespace Admin.Application.Dtos
{
    public class TrainDto : BaseEntity
    {
        public string TrainName { get; set; }
        public Track Track { get; set; }
        public ICollection<TrainCarDto>? TrainCars { get; set; }
        public TrainStatusDto? Status { get; set; }
    }
}