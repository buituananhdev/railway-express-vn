using Common.Application.Repositories;
using Payment.Domain.Entities;

namespace Payment.Application.Repositories;
public interface IPaymentRepository : IBaseRepository<PaymentRecord>
{
}
