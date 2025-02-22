using System.Net;
using Common.Domain.Enums;

namespace Common.Application.Exceptions;

public sealed class UnauthorizedAccessException : CustomException
{
    private const string DefaultMessage = "Unauthorized access. Please authenticate to access this resource.";

    public UnauthorizedAccessException(string? message = DefaultMessage)
        : base(HttpStatusCode.Unauthorized, ErrorCodeEnum.Unauthorized, message)
    {
    }
}
