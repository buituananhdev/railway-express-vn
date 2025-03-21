using Common.Application.Dtos;
using Payment.Domain.Enums;

namespace Payment.Application.Dtos;
public class PaymentRecordDto : BaseDto
{
    public string PaymentNo { get; set; }
    public List<Guid> TicketIds { get; set; }
    public string? Description { get; set; }
    public PaymentStatusEnum Status { get; set; }
    public string? IpAddress { get; set; }
    public int? StatusCode { get; set; }
    public string? TransactionNumber { get; set; }
    public string? BankName { get; set; }
    public decimal? Amount { get; set; }
}
