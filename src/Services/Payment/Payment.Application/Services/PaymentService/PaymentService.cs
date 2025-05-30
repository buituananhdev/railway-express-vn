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
using System.Security.Cryptography;

namespace Payment.Application.Services.PaymentService;

public class PaymentService : BaseService<PaymentRecord, AddPaymentRecordDto, UpdatePaymentRecordDto, PaymentRecordDto>, IPaymentService
{
    private readonly BookingGrpcService.BookingGrpcServiceClient _bookingGrpcServiceClient;
    private readonly IPaymentUnitOfWork _unitOfWork;
    private readonly IPublishEndpoint _publishEndpoint;

    private static readonly ThreadLocal<Random> _threadLocalRandom = new(() => new Random(GetCryptoRandomSeed()));

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

            var sendETicketTask = SendETicketAsync(payment, payment.BookingOrderId);
            var publishEventTask = _publishEndpoint.Publish(new UpdateTicketStatusEvent(payment.BookingOrderId, 0));

            await Task.WhenAll(sendETicketTask, publishEventTask);
        }
        else
        {
            payment.Status = PaymentStatusEnum.Failed;
            await _publishEndpoint.Publish(new UpdateTicketStatusEvent(payment.BookingOrderId, 2));
        }

        _repository.Update(payment);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<PaymentRecordDto>(payment);
    }

    public async Task<Guid> CreateTemporaryPaymentRecordAsync(Guid bookingOrderId)
    {
        var paymentNo = GeneratePaymentNumber();

        var request = new GetTicketPriceRequest { BookingOrderId = bookingOrderId.ToString() };

        var response = await _bookingGrpcServiceClient.GetTicketPriceAsync(request,
            deadline: DateTime.UtcNow.AddSeconds(30));

        var paymentRecord = new PaymentRecord
        {
            PaymentNo = paymentNo,
            Description = $"Thanh toan cho don hang {paymentNo}",
            BookingOrderId = bookingOrderId,
            Amount = (decimal)response.Price,
            Status = PaymentStatusEnum.UnPaid,
        };

        await _unitOfWork.PaymentRepository.AddAsync(paymentRecord);
        await _unitOfWork.SaveChangesAsync();

        return paymentRecord.Id;
    }

    private static string GeneratePaymentNumber()
    {
        var randomNumber = _threadLocalRandom.Value!.Next(100000, 1000000);
        return $"PO-{randomNumber}";
    }

    private static int GetCryptoRandomSeed()
    {
        using var rng = RandomNumberGenerator.Create();
        var bytes = new byte[4];
        rng.GetBytes(bytes);
        return BitConverter.ToInt32(bytes, 0);
    }
    private async Task SendETicketAsync(PaymentRecord payment, Guid bookingOrderId)
    {
        var response = await _bookingGrpcServiceClient.GetTicketInformationAsync(
                new GetTicketInformationRequest { BookingOrderId = bookingOrderId.ToString() });
        var tickets = response.Tickets;
        foreach (var ticket in tickets)
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
