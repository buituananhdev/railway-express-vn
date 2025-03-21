using AutoMapper;
using Common.Application.Exceptions;
using Common.Application.Interfaces;
using Common.Application.Services;
using Common.Protos;
using Payment.Application.Dtos;
using Payment.Application.Repositories;
using Payment.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Payment.Application.Services.PaymentService;

public class PaymentService : BaseService<PaymentRecord, AddPaymentRecordDto, AddPaymentRecordDto, PaymentRecordDto>, IPaymentService
{
    private readonly BookingGrpcService.BookingGrpcServiceClient _bookingGrpcServiceClient;
    private readonly IPaymentUnitOfWork _unitOfWork;
    private static readonly Random _random = new Random();

    public PaymentService(
        IPaymentRepository repository,
        IPaymentUnitOfWork unitOfWork,
        IMapper mapper,
        IPaginationService paginationService,
        BookingGrpcService.BookingGrpcServiceClient bookingGrpcServiceClient)
        : base(repository, unitOfWork, mapper, paginationService)
    {
        _unitOfWork = unitOfWork;
        _bookingGrpcServiceClient = bookingGrpcServiceClient;
    }

    public override async Task<PaymentRecordDto> GetByIdAsync(Guid id)
    {
        var paymentRecord = await _unitOfWork.PaymentRepository.GetByIdAsync(id)
            ?? throw new NotFoundException($"Payment record with id {id} not found");
        return _mapper.Map<PaymentRecordDto>(paymentRecord);
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
}
