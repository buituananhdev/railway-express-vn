using System.Linq.Expressions;
using Common.Application.Dtos;
using Common.Domain.Specifications;
using Payment.Application.Dtos;
using Payment.Domain.Entities;

namespace Payment.Application.Services.PaymentService;
public interface IPaymentService
{
    Task<PaymentRecordDto> CreateAsync(AddPaymentRecordDto createDto);
    Task<PaginationResult<PaymentRecordDto>> GetListAsync(
        PaginationParams paginationParams,
        Specification<PaymentRecord>? specification = null,
        List<Expression<Func<PaymentRecord, object>>>? includes = null);
    Task<PaymentRecordDto> UpdateAsync(Guid id, UpdatePaymentRecordDto updateDto);
    Task DeleteAsync(Guid id);
    Task<PaymentRecordDto> GetByIdAsync(Guid id);
    Task<Guid> CreateTemporaryPaymentRecordAsync(List<Guid> ticketIds);
}
