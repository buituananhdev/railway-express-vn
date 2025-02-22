using Common.Application.Interfaces;
using Serilog;

namespace Common.Infrastructure.Logging;
public class SerilogLogger : ILoggerService
{
    private readonly ILogger _logger;

    public SerilogLogger()
    {
        _logger = Log.ForContext<SerilogLogger>();
    }
    public void LogError(string message, Exception ex)
    {
        _logger.Error(ex, message);
    }

    public void LogInformation(string message)
    {
        _logger.Information(message);
    }

    public void LogWarning(string message)
    {
        _logger.Warning(message);
    }
}
