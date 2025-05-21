using System.Text;
using HtmlAgilityPack;
using Notification.Application.Interfaces;
using PuppeteerSharp;
using PuppeteerSharp.Media;

namespace Notification.Infrastructure.Services;

public class PdfGeneratorService : IPdfGenerator
{
    public async Task<byte[]> GeneratePdfFromHtmlAsync(string htmlContent)
    {
        var processedHtml = ProcessHtmlContent(htmlContent);

        await new BrowserFetcher().DownloadAsync();

        using var browser = await Puppeteer.LaunchAsync(new LaunchOptions { Headless = true, Args = new[] { "--no-sandbox", "--disable-setuid-sandbox" } });
        using var page = await browser.NewPageAsync();

        await page.SetContentAsync(processedHtml);

        var pdfOptions = new PdfOptions
        {
            Format = PaperFormat.A4,
            PrintBackground = true,
            MarginOptions = new MarginOptions
            {
                Top = "0px",
                Bottom = "0px",
                Left = "0px",
                Right = "0px"
            }
        };

        return await page.PdfDataAsync(pdfOptions);
    }

    private string ProcessHtmlContent(string originalHtml)
    {
        var htmlDoc = new HtmlDocument();
        htmlDoc.LoadHtml(originalHtml);

        var styles = htmlDoc.DocumentNode.SelectNodes("//style");
        var ticketContainer = htmlDoc.DocumentNode.SelectSingleNode("//div[@class='ticket-container']");

        var sb = new StringBuilder();
        sb.Append("<html><head><meta charset='UTF-8'>");

        if (styles != null)
        {
            foreach (var styleNode in styles)
            {
                sb.Append(styleNode.OuterHtml);
            }
        }

        sb.Append("</head><body style='margin:0'>");

        if (ticketContainer != null)
        {
            sb.Append(ticketContainer.OuterHtml);
        }

        sb.Append("</body></html>");

        return sb.ToString();
    }
}
