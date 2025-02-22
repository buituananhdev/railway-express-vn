using Common.Domain.Specifications;
using System.Linq.Expressions;

namespace UserManagement.Domain.Specifications;
public class PassengerEmailSpecification : Specification<UserManagement.Domain.Entities.Passenger>
{
    private readonly string _email;

    public PassengerEmailSpecification(string email)
    {
        _email = email;
    }

    public override Expression<Func<UserManagement.Domain.Entities.Passenger, bool>> ToExpression()
    {
        return user => user.Email == _email;
    }
}
