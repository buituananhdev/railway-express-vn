using Booking.Application.Services;
using Common.Contracts.Events;
using MassTransit;

namespace Booking.Infrastructure.Consumers;
public sealed class SendETicketConsumer : IConsumer<SendETicketEvent>
{
    private readonly ITicketService _ticketService;

    public SendETicketConsumer(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }
    public async Task Consume(ConsumeContext<SendETicketEvent> context)
    {
        await _ticketService.SendETicketAsync(context.Message.BookingOrderId);
    }
}
