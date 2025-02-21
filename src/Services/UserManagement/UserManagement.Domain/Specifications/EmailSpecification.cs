using Common.Domain.Specifications;
using System.Linq.Expressions;
using UserManagement.Domain.Entities;

namespace UserManagement.Domain.Specifications
{
    public class EmailSpecification : Specification<Passenger>
    {
        private readonly string _email;

        public EmailSpecification(string email)
        {
            _email = email;
        }

        public override Expression<Func<Passenger, bool>> ToExpression()
        {
            return user => user.Email == _email;
        }
    }
}
