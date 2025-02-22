namespace Admin.Domain.Enums;
public enum Track
{
    Track1 = 1,
    Track2 = 2,
    Track3 = 3
}

public enum SeatType
{
    Standard,
    Business
}

public enum TrainStatus
{
    InTransit,
    AtStation,
    Delayed,
    AwaitingDeparture,
    Reversing,
    Cancelled,
    Faulty
}
