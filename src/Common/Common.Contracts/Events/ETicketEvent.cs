namespace Common.Contracts.Events;
public record ETicketEvent
(
    string TicketNumber,
    string PassengerName,
    string Email,
    string TicketType,
    JourneyInfo Journey,
    DateTime BookingDate,
    string LogoUrl,
    string QrCodeUrl
);

public record JourneyInfo(
    string DepartureStation,
    string ArrivalStation,
    DateTime DepartureDate,
    DateTime ArrivalDate,
    TimeSpan DepartureTime,
    TimeSpan ArrivalTime,
    string TrainNumber,
    string CarriageNumber,
    string SeatNumber
);
