namespace Notification.Application.Interfaces;
public interface IPdfGenerator
{
    Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent);
}
