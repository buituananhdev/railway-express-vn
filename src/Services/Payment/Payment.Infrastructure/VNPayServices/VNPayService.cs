using System.Net;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Payment.Application.Services.PaymentService;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;
using VNPAY.NET.Utilities;

namespace Payment.Infrastructure.VNPayServices;
public class VNPayService : IVNPayService
{
    private readonly IPaymentService _paymentService;
    private readonly IConfiguration _configuration;
    private readonly IVnpay _vnpay;

    public VNPayService(IConfiguration configuration, IVnpay vnpay, IPaymentService paymentService)
    {
        _paymentService = paymentService;
        _configuration = configuration;
        _vnpay = vnpay;
        _vnpay.Initialize(_configuration["Vnpay:TmnCode"], _configuration["Vnpay:HashSecret"], _configuration["Vnpay:BaseUrl"], _configuration["Vnpay:ReturnUrl"]);
    }
    public async Task<string> GeneratePaymentUrl(Guid paymentId, string ipAddress)
    {

        var paymentRecord = await _paymentService.GetByIdAsync(paymentId);
        try
        {
            var request = new PaymentRequest
            {
                PaymentId = DateTime.Now.Ticks,
                Money = (double)paymentRecord.Amount,
                Description = paymentRecord.Description,
                IpAddress = ipAddress,
                BankCode = BankCode.ANY,
                CreatedDate = DateTime.Now,
                Currency = Currency.VND,
                Language = DisplayLanguage.Vietnamese
            };

            var paymentUrl = _vnpay.GetPaymentUrl(request);
            return paymentUrl;
        }
        catch (Exception)
        {
            throw;
        }
    }

    public PaymentResult GetPaymentResult(IQueryCollection parameters)
    {
        try
        {
            throw new NotImplementedException();
        }
        catch (Exception)
        {
            throw;
        }
    }
}
