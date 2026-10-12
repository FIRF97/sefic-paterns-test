using System.Linq.Expressions;

namespace Sefic.Partners.Shared.Helper;

public static class ExpressionExtensions
{
    public static Expression<Func<T, bool>> AndAlso<T>(
        this Expression<Func<T, bool>> expr1,
        Expression<Func<T, bool>> expr2)
    {
        var parameter = expr1.Parameters[0];
        var visitor = new ReplaceParameterVisitor(expr2.Parameters[0], parameter);
        var body2 = visitor.Visit(expr2.Body);
        var combined = Expression.AndAlso(expr1.Body, body2);

        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }

    private class ReplaceParameterVisitor : ExpressionVisitor
    {
        private readonly ParameterExpression _source;
        private readonly ParameterExpression _target;

        public ReplaceParameterVisitor(ParameterExpression source, ParameterExpression target)
        {
            _source = source;
            _target = target;
        }

        protected override Expression VisitParameter(ParameterExpression node)
        {
            return node == _source ? _target : base.VisitParameter(node);
        }
    }
}
