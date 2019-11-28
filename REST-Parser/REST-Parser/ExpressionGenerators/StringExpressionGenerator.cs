using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Linq.Expressions;
using System.Reflection;

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
                    case "contains":
                        return this.GetContainsExpression(parameter, field, value);
                    default:
                        throw new REST_InvalidOperatorException(field, restOperator);
                }
            }
            catch(REST_InvalidOperatorException ex)
            {
                throw ex;
            }
            catch (Exception)
            {
                throw new REST_InvalidFieldnameException(string.Format("field={0} value={1}", field, value));
            }
        }

        private Expression<Func<T, bool>> GetContainsExpression(ParameterExpression parameter, string field, string value)
        {
            Expression property = Expression.Property(parameter, field);
            ConstantExpression search = Expression.Constant(value, typeof(string));
            MethodInfo method = typeof(string).GetMethod("Contains", new[] { typeof(string) });
            var containsMethodExp = Expression.Call(property, method, search);
            return Expression.Lambda<Func<T, bool>>(containsMethodExp, parameter);

        }
    }
}
