using Common.Domain.Enums;

namespace Common.Application.Exceptions;
public class CustomException : Exception
{
    public int StatusCode { get; }
    public ErrorCodeEnum ErrorCode { get; }

    public CustomException(int statusCode, ErrorCodeEnum errorCode, string message) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = errorCode;
    }
    public CustomException(int statusCode, string message) : base(message)
    {
        StatusCode = statusCode;
        ErrorCode = ErrorCodeEnum.ServerError;
    }

}
