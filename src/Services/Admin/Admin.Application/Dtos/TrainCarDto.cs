using Admin.Domain.Enums;

namespace Admin.Application.Dtos
{
    public class TrainCarDto
    {
        public string? CarNumber { get; set; }
        public SeatType SeatType { get; set; }
        public int TotalSeats { get; set; }
    }
}
