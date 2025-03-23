using System;
using DinkToPdf;
using DinkToPdf.Contracts;
using Notification.Application.Interfaces;
using HtmlAgilityPack;
using System.Text;

namespace Notification.Infrastructure.Services;

public class PdfGeneratorService : IPdfGenerator
{
    private readonly IConverter _convert;

    public PdfGeneratorService(IConverter convert)
    {
        _convert = convert;
    }

    public byte[] GeneratePdfFromHtml(string htmlContent)
    {
        var processedHtml = ProcessHtmlContent(htmlContent);

        var doc = new HtmlToPdfDocument()
        {
            GlobalSettings = {
                ColorMode = ColorMode.Color,
                Orientation = Orientation.Portrait,
                PaperSize = PaperKind.A4,
                Margins = new MarginSettings { Top = 5, Bottom = 5, Left = 5, Right = 5 },
                DPI = 300
            },
            Objects = {
                new ObjectSettings() {
                    HtmlContent = processedHtml,
                    WebSettings = {
                        DefaultEncoding = "utf-8",
                        EnableIntelligentShrinking = false
                    },
                    UseLocalLinks = true,
                    LoadSettings = {
                        BlockLocalFileAccess = false
                    }
                }
            }
        };
        return _convert.Convert(doc);
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
