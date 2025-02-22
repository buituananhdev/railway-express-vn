namespace Common.Application.Interfaces;
public interface ILoggerService
{
    void LogInformation(string message);
    void LogError(string message, Exception ex);
    void LogWarning(string message);
}
