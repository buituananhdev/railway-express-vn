using System.Net;
using Common.Domain.Enums;

namespace Common.Application.Exceptions;
public sealed class ConflictException : CustomException
{
    public ConflictException(string message) : base(HttpStatusCode.Conflict, ErrorCodeEnum.Conflict, message)
    {
    }
}
