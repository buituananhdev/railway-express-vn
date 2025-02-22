using System.Linq.Expressions;
using Common.Domain.Specifications;

namespace UserManagement.Domain.Specifications;
public class AccountEmailSpecification : Specification<UserManagement.Domain.Entities.UserAccount>
{
    private readonly string _email;

    public AccountEmailSpecification(string email)
    {
        _email = email;
    }

    public override Expression<Func<UserManagement.Domain.Entities.UserAccount, bool>> ToExpression()
    {
        return user => user.Email == _email;
    }
}
