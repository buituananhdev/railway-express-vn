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

        var entity = await _unitOfWork.PaymentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Entity with id {id} not found");

        if (entity.IsSentETicket)
        {
            throw new Exception("Cannot update payment record after sending e-ticket");
        }

        _mapper.Map(updatePayment, entity);
        if (updatePayment.VnpResponseCode == 00)
        {
            entity.Status = PaymentStatusEnum.Paid;
            entity.IsSentETicket = true;
            await SendETicketAsync(id);
            await _publishEndpoint.Publish(new UpdateTicketStatusEvent(entity.TicketIds, 0));
        } else
        {
            entity.Status = PaymentStatusEnum.Failed;
            await _publishEndpoint.Publish(new UpdateTicketStatusEvent(entity.TicketIds, 2));
        }

        _repository.Update(entity);
        await _unitOfWork.SaveChangesAsync();
        return _mapper.Map<PaymentRecordDto>(entity);
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

    private async Task SendETicketAsync(Guid id)
    {

        var sampleEvent = new PaymentSuccessEvent(
            PassengerName: "Nguyen Van A",
            Email: "anhaanh2003@gmail.com",
            PhoneNumber: "0987654321",
            TicketNumber: "TCK123456",
            BookingCode: "BK987654",
            TicketType: "First Class",
            OutgoingJourney: new JourneyInfo(
                DepartureStation: "Hanoi",
                ArrivalStation: "Ho Chi Minh City",
                DepartureDate: new DateTime(2025, 4, 15),
                ArrivalDate: new DateTime(2025, 4, 16),
                DepartureTime: new TimeSpan(8, 30, 0),
                ArrivalTime: new TimeSpan(18, 45, 0),
                TrainNumber: "SE1",
                CarriageNumber: "A5",
                SeatNumber: "12B"
            ),
            ReturnJourney: new JourneyInfo(
                DepartureStation: "Ho Chi Minh City",
                ArrivalStation: "Hanoi",
                DepartureDate: new DateTime(2025, 4, 20),
                ArrivalDate: new DateTime(2025, 4, 21),
                DepartureTime: new TimeSpan(7, 0, 0),
                ArrivalTime: new TimeSpan(17, 15, 0),
                TrainNumber: "SE2",
                CarriageNumber: "B3",
                SeatNumber: "8A"
            ),
            TotalPrice: 2500000m,
            PaymentMethod: "Credit Card",
            BookingDate: DateTime.UtcNow,
            QrCodeUrl: "https://example.com/qrcode.png",
            LogoUrl: "https://example.com/logo.png",
            HasReturnJourney: true
        );

        await _publishEndpoint.Publish(sampleEvent);
    }
}
