namespace Notification.Application.Interfaces;
public interface IPdfGenerator
{
    public byte[] GeneratePdfFromHtml(string htmlContent);
}
