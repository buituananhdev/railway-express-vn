namespace Common.Contracts;
public record PaymentSuccessEvent
(
    string PassengerName,
    string Email,
    string PhoneNumber,
    string TicketNumber,
    string BookingCode,
    string TicketType,
    JourneyInfo OutgoingJourney,
    JourneyInfo? ReturnJourney,
    decimal TotalPrice,
    string PaymentMethod,
    DateTime BookingDate,
    string QrCodeUrl,
    string LogoUrl,
    bool HasReturnJourney = false
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
