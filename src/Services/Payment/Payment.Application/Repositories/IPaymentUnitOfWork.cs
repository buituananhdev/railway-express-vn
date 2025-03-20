using Common.Application.Repositories;

namespace Payment.Application.Repositories;
public interface IPaymentUnitOfWork : IUnitOfWork
{
    IPaymentRepository PaymentRepository { get; }
}
