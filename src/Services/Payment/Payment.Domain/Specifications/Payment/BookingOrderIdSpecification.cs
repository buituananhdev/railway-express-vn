using System.Linq.Expressions;
using Common.Domain.Specifications;
using Payment.Domain.Entities;

namespace Payment.Domain.Specifications;
public class BookingOrderIdSpecification : Specification<PaymentRecord>
{
    private readonly Guid _bookingOrderId;

    public BookingOrderIdSpecification(Guid bookingOrderId)
    {
        _bookingOrderId = bookingOrderId;
    }

    public override Expression<Func<PaymentRecord, bool>> ToExpression()
    {
        return payment => payment.BookingOrderId == _bookingOrderId;
    }
}

