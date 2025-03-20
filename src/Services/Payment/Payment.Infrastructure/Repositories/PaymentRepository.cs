using AutoMapper;
using Common.Infrastructure;
using Common.Infrastructure.Repositories;
using Payment.Application.Repositories;
using Payment.Domain.Entities;

namespace Payment.Infrastructure.Repositories;
public class PaymentRepository : BaseRepository<PaymentRecord>, IPaymentRepository
{
    public PaymentRepository(IDataContext context, IMapper mapper) : base(context, mapper)
    {
    }
}
