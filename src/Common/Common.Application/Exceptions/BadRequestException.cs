using System.Net;
using Common.Domain.Enums;

namespace Common.Application.Exceptions;
public sealed class BadRequestException : CustomException
{
    public BadRequestException(string message) : base(HttpStatusCode.BadRequest, ErrorCodeEnum.InvalidRequest, message)
    {
    }
}
