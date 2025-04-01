namespace Common.Contracts.Events;
public record UpdateTicketStatusEvent
(
    List<Guid> TicketIds,
    int Status
);
