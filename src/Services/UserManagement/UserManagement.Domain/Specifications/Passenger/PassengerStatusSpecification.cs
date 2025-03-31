using Common.Domain;
using Common.Domain.Specifications;
using System.Linq.Expressions;

namespace UserManagement.Domain.Specifications;
public class PassengerIsActiveSpecification : Specification<UserManagement.Domain.Entities.Passenger>
{
    private readonly StatusEnum _status;

    public PassengerIsActiveSpecification(StatusEnum status)
    {
        _status = status;
    }

    public override Expression<Func<UserManagement.Domain.Entities.Passenger, bool>> ToExpression()
    {
        return user => user.UserAccount.Status == _status;
    }
}
