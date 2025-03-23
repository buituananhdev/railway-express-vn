using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MassTransit;
using Notification.Application.Dtos;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Events;

namespace Notification.Infrastructure.Services;
public class ETicketService : IETicketService
{
    private readonly ITemplateService _templateService;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly IBus _bus;
    private readonly IEmailService _emailService;
    public ETicketService(
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

    public async Task SendTicketAsync(ETicketDto ticket)
    {
        var htmlContent = await _templateService.RenderTemplateAsync("TicketTemplate", ticket);

        var pdfBytes = _pdfGenerator.GeneratePdfFromHtml(htmlContent);

        await _bus.Publish(new TicketCreatedEvent(
            ticket.Email,
            "Your E-Ticket",
            "Please find your ticket attached",
            pdfBytes
        ));
    }
}
