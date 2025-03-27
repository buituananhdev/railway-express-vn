using Microsoft.AspNetCore.Mvc;
using Payment.Domain.Enums;
using Payment.Infrastructure.VNPayServices;
using VNPAY.NET.Utilities;

namespace Payment.API.Controllers;
[Route("api/vnpay")]
[ApiController]
public class VNPayController : ControllerBase
{
    private readonly IVNPayService _vnPayService;
    public VNPayController(IVNPayService vnPayService)
    {
        _vnPayService = vnPayService;
    }

    [HttpGet("url")]
    public async Task<IActionResult> CreatePaymentUrl(Guid paymentID, PaymentTypeEnum paymentType)
    {
        var ipAddress = NetworkHelper.GetIpAddress(HttpContext);
        var paymentUrl = await _vnPayService.GeneratePaymentUrl(paymentID, ipAddress, paymentType);
        return Ok(paymentUrl);
    }

    [HttpGet("callback")]
    public ActionResult PaymentCallback()
    {
        var paymentResult = _vnPayService.GetPaymentResult(Request.Query);

        return Ok(paymentResult);
    }

    //[HttpGet("ipn")]
    //public ActionResult PaymentIpnNotification()
    //{
    //    try
    //    {
    //        if (Request.QueryString.HasValue)
    //        {
    //            var paymentResult = _vnPayService.GetPaymentResult(Request.Query);

    //            if (paymentResult.IsSuccess)
    //            {
    //                // Cập nhật trạng thái đơn hàng trong database
    //                // UpdateOrderStatus(paymentResult.PaymentId, "Paid");

    //                return Ok(); // Trả về mã 200 để VNPAY biết đã nhận thông báo
    //            }

    //            // Xử lý thanh toán thất bại
    //            // UpdateOrderStatus(paymentResult.PaymentId, "Failed");

    //            return Ok(); // Vẫn trả về OK để VNPAY biết đã nhận thông báo
    //        }

    //        return NotFound();
    //    }
    //    catch (Exception)
    //    {
    //        return BadRequest(new { vnp_ResponseCode = "97" }); // Mã lỗi theo quy định của VNPAY
    //    }
    //}
}
