using Common.Infrastructure;
using Common.Infrastructure.Repositories;
using Payment.Application.Repositories;

namespace Payment.Infrastructure.Repositories;
public class PaymentUnitOfWork : UnitOfWork, IPaymentUnitOfWork
{
    public IPaymentRepository PaymentRepository { get; private set; }
    public PaymentUnitOfWork(IDataContext context, IPaymentRepository paymentRepository) : base(context)
    {
        PaymentRepository = paymentRepository;
    }
}
