using Microsoft.AspNetCore.Mvc;
using Payment.Application.Dtos;
using Payment.Application.Services.PaymentService;

namespace Payment.API.Controllers;
[Route("v1/payment-records")]
[ApiController]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    public PaymentController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost]
    public async Task<IActionResult> CreatePaymentAsync([FromBody]Guid bookingOrderId)
    {
        var result = await _paymentService.CreateTemporaryPaymentRecordAsync(bookingOrderId);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPaymentAsync(Guid id)
    {
        var result = await _paymentService.GetByIdAsync(id);
        return Ok(result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdatePaymentAsync(Guid id, [FromBody] UpdatePaymentRecordDto updateDto)
    {
        var result = await _paymentService.UpdateAsync(id, updateDto);
        return Ok(result);
    }

    [HttpGet("booking-order/{bookingOrderId}")]
    public async Task<IActionResult> GetPaymentByBookingOrderIdAsync(Guid bookingOrderId)
    {
        var result = await _paymentService.GetPaymentByBookingOrderIdAsync(bookingOrderId);
        return Ok(result);
    }
}
