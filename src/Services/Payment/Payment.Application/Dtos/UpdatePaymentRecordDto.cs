using Payment.Application.Utils;
using System.Text.Json.Serialization;
using Payment.Domain.Enums;

namespace Payment.Application.Dtos;
public class UpdatePaymentRecordDto
{
    [JsonConverter(typeof(VnpResponseCodeConverter))]
    public VnpResponseCode? VnpResponseCode { get; set; }
    public string? TransactionNumber { get; set; }
    public string? BankName { get; set; }
}
