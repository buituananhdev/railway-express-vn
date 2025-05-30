namespace Common.Contracts.Events;
public record UpdateTicketStatusEvent
(
    Guid BookingOrderId,
    int Status
);
