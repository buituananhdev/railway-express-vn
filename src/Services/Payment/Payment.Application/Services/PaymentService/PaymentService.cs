using AutoMapper;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Application.Services;
using Common.Contracts.Events;
using Common.Protos;
using MassTransit;
using Payment.Application.Dtos;
using Payment.Application.Repositories;
using Payment.Domain.Entities;
using Payment.Domain.Enums;

namespace Payment.Application.Services.PaymentService;

public class PaymentService : BaseService<PaymentRecord, AddPaymentRecordDto, UpdatePaymentRecordDto, PaymentRecordDto>, IPaymentService
{
    private readonly BookingGrpcService.BookingGrpcServiceClient _bookingGrpcServiceClient;
    private readonly IPaymentUnitOfWork _unitOfWork;
    private static readonly Random _random = new Random();
    private readonly IPublishEndpoint _publishEndpoint;
    public PaymentService(
        IPaymentRepository repository,
        IPaymentUnitOfWork unitOfWork,
        IMapper mapper,
        IPaginationService paginationService,
        BookingGrpcService.BookingGrpcServiceClient bookingGrpcServiceClient,
        IPublishEndpoint publishEndpoint)
        : base(repository, unitOfWork, mapper, paginationService)
    {
        _unitOfWork = unitOfWork;
        _bookingGrpcServiceClient = bookingGrpcServiceClient;
        _publishEndpoint = publishEndpoint;
    }

    public override async Task<PaymentRecordDto> UpdateAsync(Guid id, UpdatePaymentRecordDto updatePayment)
    {

        var payment = await _unitOfWork.PaymentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Entity with id {id} not found");

        if (payment.IsSentETicket)
        {
            return _mapper.Map<PaymentRecordDto>(payment);
        }

        _mapper.Map(updatePayment, payment);
        if (updatePayment.VnpResponseCode == 00)
        {
            payment.Status = PaymentStatusEnum.Paid;
            payment.IsSentETicket = true;
            await SendETicketAsync(payment, payment.TicketIds);
            await _publishEndpoint.Publish(new UpdateTicketStatusEvent(payment.TicketIds, 0));
        }
        else
        {
            payment.Status = PaymentStatusEnum.Failed;
            await _publishEndpoint.Publish(new UpdateTicketStatusEvent(payment.TicketIds, 2));
        }

        _repository.Update(payment);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<PaymentRecordDto>(payment);
    }


    public async Task<Guid> CreateTemporaryPaymentRecordAsync(List<Guid> ticketIds)
    {
        if (ticketIds == null || !ticketIds.Any())
        {
            throw new ArgumentException("Ticket IDs cannot be null or empty", nameof(ticketIds));
        }

        var paymentNo = GeneratePaymentNumber();

        var tasks = new List<Task<GetTicketPriceResponse>>();

        foreach (var ticketId in ticketIds)
        {
            var call = _bookingGrpcServiceClient.GetTicketPriceAsync(
                new GetTicketPriceRequest { TicketId = ticketId.ToString() });
            tasks.Add(call.ResponseAsync);
        }

        var results = await Task.WhenAll(tasks);

        decimal totalAmount = results.Sum(result => (decimal)result.Price);

        var paymentRecord = new PaymentRecord
        {
            PaymentNo = paymentNo,
            Description = $"Thanh toan cho don hang {paymentNo}",
            TicketIds = ticketIds,
            Amount = totalAmount,
            Status = Domain.Enums.PaymentStatusEnum.UnPaid,
        };

        await _unitOfWork.PaymentRepository.AddAsync(paymentRecord);
        await _unitOfWork.SaveChangesAsync();
        return paymentRecord.Id;
    }

    private static string GeneratePaymentNumber()
    {
        // Use thread-safe random number generation
        int randomNumber;
        lock (_random)
        {
            randomNumber = _random.Next(100000, 1000000);
        }

        return $"PO-{randomNumber}";
    }

    private async Task SendETicketAsync(PaymentRecord payment, List<Guid> ticketIds)
    {
        var ticketInfos = new List<GetTicketInformationResponse>();
        foreach (var ticketId in ticketIds)
        {
            var ticketInfo = await _bookingGrpcServiceClient.GetTicketInformationAsync(
                new GetTicketInformationRequest { TicketId = ticketId.ToString() });
            ticketInfos.Add(ticketInfo);
        }

        foreach (var ticket in ticketInfos)
        {
            var mainPassenger = ticket.PassengerDetails
                .Where(p => (bool)p.IsMainPassenger)
                .FirstOrDefault();

            var event1 = new PaymentSuccessEvent(
                TicketNumber: ticket.TicketNumber,
                PassengerName: mainPassenger.FirstName + mainPassenger.LastName,
                Email: mainPassenger.Email,
                TicketType: "First Class",
                BookingCode: payment.PaymentNo,
                Journey: new JourneyInfo(
                    DepartureStation: "Hanoi",
                    ArrivalStation: "Ho Chi Minh City",
                    DepartureDate: ticket.JourneyDate.ToDateTime(),
                    ArrivalDate: ticket.JourneyDate.ToDateTime(),
                    DepartureTime: new TimeSpan(8, 30, 0),
                    ArrivalTime: new TimeSpan(18, 45, 0),
                    TrainNumber: "SE1",
                    CarriageNumber: "A5",
                    SeatNumber: "12B"
                ),
                BookingDate: DateTime.UtcNow,
                QrCodeUrl: "https://example.com/qrcode.png",
                LogoUrl: "https://example.com/logo.png"
            );
            await _publishEndpoint.Publish(event1);
        }
    }
}
