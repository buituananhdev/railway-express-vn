using Booking.Application.Services;
using Booking.Domain.Enums;
using Common.Contracts.Events;
using MassTransit;

namespace Booking.Infrastructure.Consumers;
public sealed class UpdateTicketStatusConsumer : IConsumer<UpdateTicketStatusEvent>
{
    private readonly ITicketService _ticketService;

    public UpdateTicketStatusConsumer(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }
    public async Task Consume(ConsumeContext<UpdateTicketStatusEvent> context)
    {
        foreach (var ticketId in context.Message.TicketIds)
        {
            await _ticketService.UpdateTicketStatusAsync(ticketId, (TicketStatusEnum)context.Message.Status);
        }
    }
}
