using Microsoft.AspNetCore.Http;
using Payment.Domain.Enums;
using VNPAY.NET.Models;

namespace Payment.Infrastructure.VNPayServices;
public interface IVNPayService
{
    Task<string> GeneratePaymentUrl(Guid paymentId, string ipAddress, PaymentTypeEnum paymentType);
    PaymentResult GetPaymentResult(IQueryCollection parameters);
}
