using System.Net;
using Common.Domain.Enums;

namespace Common.Application.Exceptions;
public class CustomException : Exception
{
    public HttpStatusCode StatusCode { get; }
    public ErrorCodeEnum ErrorCode { get; }

    public CustomException(HttpStatusCode statusCode, ErrorCodeEnum errorCode, string message) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }

    public CustomException(HttpStatusCode statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = ErrorCodeEnum.ServerError;
    }

}
