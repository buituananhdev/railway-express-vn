namespace Auth.Application.Services;
public interface IGreeterGrpcService
{
    Task<string> SayHelloAsync(string name, CancellationToken cancellationToken);
}
