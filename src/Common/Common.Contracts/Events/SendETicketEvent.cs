namespace Common.Contracts.Events;
public record SendETicketEvent(
    Guid BookingOrderId
);
