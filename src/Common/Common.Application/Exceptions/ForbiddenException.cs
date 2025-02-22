using System.Net;
using Common.Domain.Enums;

namespace Common.Application.Exceptions;

public sealed class ForbiddenException : CustomException
{
    private const string DEFAULT_MESSAGE = "You do not have permission to access this resource.";

    public ForbiddenException(string? message = DEFAULT_MESSAGE)
        : base(HttpStatusCode.Forbidden, ErrorCodeEnum.Forbidden, message)
    {
    }
}
