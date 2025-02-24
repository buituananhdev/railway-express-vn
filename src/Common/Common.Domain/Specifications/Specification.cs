using System.Linq.Expressions;

namespace Common.Domain.Specifications;
public abstract class Specification<T>
{
    public abstract Expression<Func<T, bool>> ToExpression();

    public bool IsSatisfiedBy(T entity)
    {
        var predicate = ToExpression().Compile();
        return predicate(entity);
    }

    public Specification<T> And(Specification<T> specification)
    {
        return new AndSpecification<T>(this, specification);
    }

    public Specification<T> Or(Specification<T> specification)
    {
        return new OrSpecification<T>(this, specification);
    }
}

public class AndSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;


    public AndSpecification(Specification<T> left, Specification<T> right)
    {
        _right = right;
        _left = left;
    }


    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var paramExpr = Expression.Parameter(typeof(T));
        var exprBody = Expression.AndAlso(leftExpression.Body, rightExpression.Body);
        exprBody = (BinaryExpression)new ParameterReplacer(paramExpr).Visit(exprBody);
        var finalExpr = Expression.Lambda<Func<T, bool>>(exprBody, paramExpr);

        return finalExpr;
    }
}

public class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;


    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        _right = right;
        _left = left;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();
        var paramExpr = Expression.Parameter(typeof(T));
        var exprBody = Expression.OrElse(leftExpression.Body, rightExpression.Body);
        exprBody = (BinaryExpression)new ParameterReplacer(paramExpr).Visit(exprBody);
        var finalExpr = Expression.Lambda<Func<T, bool>>(exprBody, paramExpr);

        return finalExpr;
    }
}

public class AndSpecificationMultiple<T> : Specification<T>
{
    private readonly IEnumerable<Specification<T>> _specifications;

    public AndSpecificationMultiple(IEnumerable<Specification<T>> specifications)
    {
        _specifications = specifications;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var paramExpr = Expression.Parameter(typeof(T));
        Expression body = _specifications.First().ToExpression().Body;

        foreach (var specification in _specifications.Skip(1))
        {
            var nextExpression = specification.ToExpression();
            body = Expression.AndAlso(body, nextExpression.Body);
            body = new ParameterReplacer(paramExpr).Visit(body);
        }

        return Expression.Lambda<Func<T, bool>>(body, paramExpr);
    }
}

internal class ParameterReplacer : ExpressionVisitor
{

    private readonly ParameterExpression _parameter;

    protected override Expression VisitParameter(ParameterExpression node)
        => base.VisitParameter(_parameter);

    internal ParameterReplacer(ParameterExpression parameter)
    {
        _parameter = parameter;
    }
}
