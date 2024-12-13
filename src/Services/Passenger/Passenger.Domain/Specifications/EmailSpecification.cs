using Common.Domain.Specifications;
using System.Linq.Expressions;

namespace Passenger.Domain.Specifications
{
    public class EmailSpecification : Specification<Passenger.Domain.Entities.Passenger>
    {
        private readonly string _email;

        public EmailSpecification(string email)
        {
            _email = email;
        }

        public override Expression<Func<Passenger.Domain.Entities.Passenger, bool>> ToExpression()
        {
            return user => user.Email == _email;
        }
    }
}
