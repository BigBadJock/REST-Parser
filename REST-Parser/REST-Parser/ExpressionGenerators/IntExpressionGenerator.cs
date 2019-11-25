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
                int v = int.Parse(value);

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
                    case "gt":
                        return Expression.Lambda<Func<T, bool>>(
                            Expression.GreaterThan(Expression.PropertyOrField(parameter, field), Expression.Constant(v)),
                            parameter);
                    case "ge":
                        return Expression.Lambda<Func<T, bool>>(
                            Expression.GreaterThanOrEqual(Expression.PropertyOrField(parameter, field), Expression.Constant(v)),
                            parameter);
                    default:
                        throw new REST_InvalidOperatorException(field, restOperator);
                }
            }
            catch (REST_InvalidOperatorException ex)
            {
                throw ex;
            }
            catch (Exception)
            {
                throw new REST_InvalidValueException(field, value);
            }
        }
    }
}
