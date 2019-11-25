using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Linq.Expressions;

namespace REST_Parser.ExpressionGenerators
{
    public class IntExpressionGenerator<T> : IIntExpressionGenerator<T>
    {
        public Expression<Func<T, bool>> GetExpression(string restOperator, ParameterExpression parameter, string field, string value)
        {
            try
            {

                int.TryParse(value, out int v);

                switch (restOperator)
                {
                    case "eq":
                        return Expression.Lambda<Func<T, bool>>(
                            Expression.Equal(Expression.PropertyOrField(parameter, field), Expression.Constant(v)),
                            parameter);
                    case "ne":
                        return Expression.Lambda<Func<T, bool>>(
                            Expression.NotEqual(Expression.PropertyOrField(parameter, field), Expression.Constant(v)),
                            parameter);
                    default:
                        return null;

                }
            }
            catch (Exception)
            {
                throw new InvalidRestException(string.Format("field={0} value={1}", field, value));
            }
        }
    }
}
