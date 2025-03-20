namespace Payment.Application.Dtos;
public class PaymentRequestModel
{
    public string OrderId { get; set; }
    public decimal Amount { get; set; }
}
