using Microsoft.AspNetCore.Mvc;
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
    public async Task<IActionResult> CreatePayment(List<Guid> ticketIds)
    {
        var result = await _paymentService.CreateTemporaryPaymentRecordAsync(ticketIds);
        return Ok(result);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetPayment(Guid id)
    {
        var result = await _paymentService.GetByIdAsync(id);
        return Ok(result);
    }
}
