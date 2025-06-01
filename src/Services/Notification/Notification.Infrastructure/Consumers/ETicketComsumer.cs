using Common.Contracts.Events;
using MassTransit;
using Notification.Application.Interfaces;
using Notification.Infrastructure.Events;

namespace Notification.Infrastructure.Consumers;
public sealed class ETicketComsumer : IConsumer<ETicketEvent>
{
    private readonly ITemplateService _templateService;
    private readonly IPdfGenerator _pdfGenerator;
    private readonly IBus _bus;
    private readonly IEmailService _emailService;
    public ETicketComsumer(
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

    public async Task Consume(ConsumeContext<ETicketEvent> context)
    {
        var htmlContent = await _templateService.RenderTemplateAsync("TicketTemplate", context.Message);

        var pdfBytes = await _pdfGenerator.GeneratePdfFromHtmlAsync(htmlContent);

        var emailBody = @"
            <p>Kính gửi Quý khách,</p>
            <p>Cảm ơn Quý khách đã tin tưởng và sử dụng dịch vụ của chúng tôi.</p>
            <p>Vé điện tử (e-ticket) của Quý khách đã được phát hành thành công. Vui lòng kiểm tra tệp đính kèm trong email này để xem chi tiết thông tin vé.</p>
            <p><strong>🔒 Lưu ý:</strong> Đây là email tự động, vui lòng không phản hồi lại email này. Nếu Quý khách có bất kỳ câu hỏi hoặc cần hỗ trợ, xin vui lòng liên hệ với bộ phận chăm sóc khách hàng qua <a href='tel:0123456789'>0123 456 789</a> hoặc <a href='mailto:hotro@duongsat.vn'>hotro@duongsat.vn</a>.</p>
            <p>Xin chân thành cảm ơn và chúc Quý khách có một trải nghiệm tuyệt vời!</p>
            <p>Trân trọng,<br/>Tổng công ty đường sắt Việt Nam</p>
        ";

        await _bus.Publish(new TicketCreatedEvent(
            context.Message.Email,
            "[No Reply] - Đặt chỗ thành công",
            emailBody,
            pdfBytes
        ));
    }
}
