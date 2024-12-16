using Admin.Domain.Entities;

namespace Admin.Application.Dtos
{
    public class TrainStatusDto
    {
        public Station Station { get; set; }
        public Admin.Domain.Enums.TrainStatus Status { get; set; }
        public string Remarks { get; set; }
    }
}
