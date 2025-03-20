using Microsoft.AspNetCore.Http;
using VNPAY.NET.Models;

namespace Payment.Infrastructure.VNPayServices;
public interface IVNPayService
{
    Task<string> GeneratePaymentUrl(Guid paymentId, string ipAddress);
    PaymentResult GetPaymentResult(IQueryCollection parameters);
}
