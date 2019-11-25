using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Linq.Expressions;

namespace REST_Parser.ExpressionGenerators
{
    public class StringExpressionGenerator<T> : IStringExpressionGenerator<T>
    {
        public Expression<Func<T, bool>> GetExpression(string restOperator, ParameterExpression parameter, string field, string value)
        {
            try
            {

                switch (restOperator)
                {
                    case "eq":
                        return Expression.Lambda<Func<T, bool>>(
                            Expression.Equal(Expression.PropertyOrField(parameter, field), Expression.Constant(value)),
                            parameter);
                    case "ne":
                        return Expression.Lambda<Func<T, bool>>(
                            Expression.NotEqual(Expression.PropertyOrField(parameter, field), Expression.Constant(value)),
                            parameter);
                    default:
                        return null;
                }
            }
            catch (Exception)
            {
                throw new REST_InvalidFieldnameException(string.Format("field={0} value={1}", field, value));
            }
        }
    }
}
