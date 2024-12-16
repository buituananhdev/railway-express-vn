using Admin.Domain.Enums;

namespace Admin.Application.Dtos
{
    public class AddTrainDto
    {
        public string TrainName { get; set; }
        public Track Track { get; set; }
    }
}
