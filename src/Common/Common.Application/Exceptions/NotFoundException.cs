using System.Net;
using Common.Domain.Enums;

namespace Common.Application.Exceptions;
public sealed class NotFoundException : CustomException
{
    public NotFoundException(string message) : base(HttpStatusCode.NotFound, ErrorCodeEnum.NotFound, message)
    {
    }
}
