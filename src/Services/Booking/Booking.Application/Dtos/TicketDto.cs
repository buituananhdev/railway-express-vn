using Booking.Domain.Entities;
using Booking.Domain.Enums;
using Common.Application.Dtos;

namespace Booking.Application.Dtos;
public class TicketDto : BaseDto
{
    public string TicketNumber { get; set; }
    public Guid? PassengerId { get; set; }
    public Guid TrainId { get; set; }
    public Seat? SeatInformation {  get; set; }
    public Guid TrainScheduleId { get; set; }
    public DateTime BookingDate { get; set; } = DateTime.Now;
    public DateTime JourneyDate { get; set; }
    public decimal TotalPrice { get; set; }
    public TicketStatusEnum Status { get; set; }
    public string? Remarks { get; set; }
    public List<PassengerInfoDto>? PassengerDetails { get; set; }
    public List<TicketSeatDto>? TicketSeats { get; set; }
}

public class Seat
{
    public TrainCar TrainCar { get; set; }
    public int SeatNumber { get; set; }
    public int Status { get; set; }
}

public class TrainCar
{
    public int? CarNumber { get; set; }
    public int SeatType { get; set; }
    public Train Train { get; set; }
}


public class Train
{
    public string TrainName { get; set; }
}

