using Common.Contracts.Events;
using MassTransit;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Events;

namespace Notification.Infrastructure.Consumers;
public sealed class PaymentSuccessComsumer : IConsumer<PaymentSuccessEvent>
{
    private readonly ITemplateService _templateService;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly IBus _bus;
    private readonly IEmailService _emailService;
    public PaymentSuccessComsumer(
        ITemplateService templateService,
        IPdfGenerator pdfGenerator,
        IBus bus,
        IEmailService emailService)
    {
        _templateService = templateService;
        _pdfGenerator = pdfGenerator;
        _bus = bus;
        _emailService = emailService;
    }

    public async Task Consume(ConsumeContext<PaymentSuccessEvent> context)
    {
        var htmlContent = await _templateService.RenderTemplateAsync("TicketTemplate", context.Message);

        var pdfBytes = _pdfGenerator.GeneratePdfFromHtml(htmlContent);

        await _bus.Publish(new TicketCreatedEvent(
            context.Message.Email,
            "Your E-Ticket",
            "Please find your ticket attached",
            pdfBytes
        ));
    }
}
