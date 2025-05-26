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
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ParameterReplacer(leftExpression.Parameters[0], parameter);
        var leftBody = leftVisitor.Visit(leftExpression.Body);

        var rightVisitor = new ParameterReplacer(rightExpression.Parameters[0], parameter);
        var rightBody = rightVisitor.Visit(rightExpression.Body);

        return Expression.Lambda<Func<T, bool>>(
            Expression.AndAlso(leftBody, rightBody),
            parameter
        );
    }
}

public class OrSpecification<T> : Specification<T>
{
    private readonly Specification<T> _left;
    private readonly Specification<T> _right;

    public OrSpecification(Specification<T> left, Specification<T> right)
    {
        _left = left;
        _right = right;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var leftExpression = _left.ToExpression();
        var rightExpression = _right.ToExpression();

        var parameter = Expression.Parameter(typeof(T));

        var leftVisitor = new ParameterReplacer(leftExpression.Parameters[0], parameter);
        var leftBody = leftVisitor.Visit(leftExpression.Body);

        var rightVisitor = new ParameterReplacer(rightExpression.Parameters[0], parameter);
        var rightBody = rightVisitor.Visit(rightExpression.Body);

        return Expression.Lambda<Func<T, bool>>(
            Expression.OrElse(leftBody, rightBody),
            parameter
        );
    }
}

public class OrSpecificationMultiple<T> : Specification<T>
{
    private readonly IEnumerable<Specification<T>> _specifications;

    public OrSpecificationMultiple(IEnumerable<Specification<T>> specifications)
    {
        _specifications = specifications;
    }

    public override Expression<Func<T, bool>> ToExpression()
    {
        var parameter = Expression.Parameter(typeof(T));
        Expression? combined = null;

        foreach (var spec in _specifications)
        {
            var expression = spec.ToExpression();
            var visitor = new ParameterReplacer(expression.Parameters[0], parameter);
            var body = visitor.Visit(expression.Body);

            combined = combined == null ? body! : Expression.OrElse(combined, body!);
        }

        return Expression.Lambda<Func<T, bool>>(combined ?? Expression.Constant(false), parameter);
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
        var parameter = Expression.Parameter(typeof(T));
        Expression combined = Expression.Constant(true);

        foreach (var spec in _specifications)
        {
            var expression = spec.ToExpression();
            var visitor = new ParameterReplacer(expression.Parameters[0], parameter);
            var body = visitor.Visit(expression.Body);
            combined = Expression.AndAlso(combined, body);
        }

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
}

internal class ParameterReplacer : ExpressionVisitor
{
    private readonly ParameterExpression _oldParameter;
    private readonly ParameterExpression _newParameter;

    public ParameterReplacer(ParameterExpression oldParameter, ParameterExpression newParameter)
    {
        _oldParameter = oldParameter;
        _newParameter = newParameter;
    }

    protected override Expression VisitParameter(ParameterExpression node)
    {
        return node == _oldParameter ? _newParameter : base.VisitParameter(node);
    }
}
