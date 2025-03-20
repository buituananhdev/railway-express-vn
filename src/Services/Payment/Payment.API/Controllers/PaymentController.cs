using Microsoft.AspNetCore.Mvc;
using Payment.Application.Services.PaymentService;
using Payment.Infrastructure.VNPayServices;
using VNPAY.NET;
using VNPAY.NET.Utilities;

namespace Payment.API.Controllers;
[Route("v1")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IVNPayService _vnPayService;
    private readonly IPaymentService _paymentService;
    public PaymentController(IVNPayService vnPayService, IVnpay vnpay, IPaymentService paymentService)
    {
        _paymentService = paymentService;
        _vnPayService = vnPayService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePayment(List<Guid> ticketIds)
    {
        var result = await _paymentService.CreateTemporaryPaymentRecordAsync(ticketIds);
        return Ok(result);
    }

    [HttpGet("vnpay-url")]
    public async Task<IActionResult> CreatePaymentUrl(Guid paymentID)
    {
        var ipAddress = NetworkHelper.GetIpAddress(HttpContext);
        var paymentUrl = await _vnPayService.GeneratePaymentUrl(paymentID, ipAddress);
        return Ok(paymentUrl);
    }

    [HttpGet("vnpay-callback")]
    public ActionResult PaymentCallback()
    {
        var paymentResult = _vnPayService.GetPaymentResult(Request.Query);

        return Ok(paymentResult);
    }

    [HttpGet("vnpay-ipn")]
    public ActionResult PaymentIpnNotification()
    {
        try
        {
            if (Request.QueryString.HasValue)
            {
                var paymentResult = _vnPayService.GetPaymentResult(Request.Query);

                if (paymentResult.IsSuccess)
                {
                    // Cập nhật trạng thái đơn hàng trong database
                    // UpdateOrderStatus(paymentResult.PaymentId, "Paid");

                    return Ok(); // Trả về mã 200 để VNPAY biết đã nhận thông báo
                }

                // Xử lý thanh toán thất bại
                // UpdateOrderStatus(paymentResult.PaymentId, "Failed");

                return Ok(); // Vẫn trả về OK để VNPAY biết đã nhận thông báo
            }

            return NotFound();
        }
        catch (Exception)
        {
            return BadRequest(new { vnp_ResponseCode = "97" }); // Mã lỗi theo quy định của VNPAY
        }
    }
}
