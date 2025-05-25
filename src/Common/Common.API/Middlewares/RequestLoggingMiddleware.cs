using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Common.API.Middlewares;
public class RequestLoggingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<RequestLoggingMiddleware> _logger;

    public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task Invoke(HttpContext context)
    {
        context.Request.EnableBuffering();

        var request = context.Request;
        var bodyStream = request.Body;

        using var reader = new StreamReader(
            bodyStream,
            encoding: Encoding.UTF8,
            detectEncodingFromByteOrderMarks: false,
            bufferSize: 1024,
            leaveOpen: true);

        var bodyText = await reader.ReadToEndAsync();
        bodyStream.Position = 0;

        _logger.LogInformation("HTTP {Method} {Path} Body: {Body}",
            request.Method,
            request.Path,
            bodyText);

        await _next(context);
    }
}
