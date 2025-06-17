using AutoMapper;
using Common.Protos;
using Grpc.Core;
using MassTransit;
using Payment.Application.Services.PaymentService;
using Payment.Domain.Enums;
using Payment.Infrastructure.VNPayServices;

namespace Payment.Infrastructure.GrpcServices;
public class PaymentService : Common.Protos.PaymentGrpcService.PaymentGrpcServiceBase
{
    private readonly IPaymentService _paymentService;
    private readonly IVNPayService _vnPayService;
    private readonly IMapper _mapper;
    public PaymentService(IPaymentService ticketService, IVNPayService vNPayService, IMapper mapper)
    {
        _paymentService = ticketService;
        _vnPayService = vNPayService;
        _mapper = mapper;
    }

    public override async Task<CreatePaymentResponse> CreatePayment(
        CreatePaymentRequest request,
        ServerCallContext context)
    {
        var payment = await _paymentService.CreateTemporaryPaymentRecordAsync(Guid.Parse(request.BookingOrderId));
        var paymentUrl = await _vnPayService.GeneratePaymentUrl(payment, "127.0.0.1", (PaymentTypeEnum)request.PaymentType);
        return new CreatePaymentResponse { PaymentUrl = paymentUrl };
    }
}
