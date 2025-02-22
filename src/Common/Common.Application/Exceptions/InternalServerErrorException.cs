using System.Net;
using Common.Domain.Enums;

namespace Common.Application.Exceptions;

public sealed class InternalServerErrorException : CustomException
{
    private const string DefaultMessage = "Internal Server Error";

    public InternalServerErrorException(string? message = DefaultMessage)
        : base(HttpStatusCode.InternalServerError, ErrorCodeEnum.InternalServerError, message)
    {
    }
}
