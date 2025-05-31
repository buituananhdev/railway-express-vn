using Booking.Domain.Entities;
using Booking.Domain.Enums;
using Common.Application.Dtos;

namespace Booking.Application.Dtos;
public class TicketDto : BaseDto
{
    public string TicketNumber { get; set; }
    public Guid? PassengerId { get; set; }
    public Guid TrainId { get; set; }
    public List<Seat>? SeatInformations {  get; set; }
    public TrainSchedule TrainSchedule { get; set; }
    public Guid TrainScheduleId { get; set; }
    public DateTime BookingDate { get; set; } = DateTime.Now;
    public DateTime JourneyDate { get; set; }
    public decimal TotalPrice { get; set; }
    public TicketStatusEnum Status { get; set; }
    public TicketTypeEnum TicketType { get; set; }
    public string? Remarks { get; set; }
    public Guid? BookingOrderId { get; set; }
    public List<PassengerInfoDto>? PassengerDetails { get; set; }
    public List<TicketSeatDto>? TicketSeats { get; set; }
}

public class Station
{
    public Guid Id { get; set; }
    public string StationName { get; set; }
    public string CityName { get; set; }
}


public class TrainSchedule
{
    public Guid Id { get; set; }
    public Station ArrivalStation { get; set; }
    public Station DepartureStation { get; set; }
    public DateTime DepartureTime { get; set; }
    public DateTime ArrivalTime { get; set; }
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

