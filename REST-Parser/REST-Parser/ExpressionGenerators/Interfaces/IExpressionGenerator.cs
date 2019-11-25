using System;
using System.Linq.Expressions;

namespace REST_Parser.ExpressionGenerators.Interfaces
{
    public interface IExpressionGenerator<T>
    {
        Expression<Func<T, bool>> GetExpression(string restOperator, ParameterExpression parameter, string field, string value);
    }
}
