using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Payment.Application.Services.PaymentService;
using Payment.Domain.Enums;
using VNPAY.NET;
using VNPAY.NET.Enums;
using VNPAY.NET.Models;

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
    public async Task<string> GeneratePaymentUrl(Guid paymentId, string ipAddress, PaymentTypeEnum paymentType)
    {

        var paymentRecord = await _paymentService.GetByIdAsync(paymentId);
        if(paymentRecord.Status != PaymentStatusEnum.UnPaid)
        {
            throw new Exception("Payment record is not valid");
        }

        try
        {
            var request = new PaymentRequest
            {
                PaymentId = paymentId,
                Money = (double)paymentRecord.Amount,
                Description = paymentRecord.Description,
                IpAddress = ipAddress,
                BankCode = paymentType == PaymentTypeEnum.BankTransfer ? BankCode.VNBANK : BankCode.INTCARD,
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
            var paymentResult = _vnpay.GetPaymentResult(parameters);
            return paymentResult;
        }
        catch (Exception)
        {
            throw;
        }
    }
}
