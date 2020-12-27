using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Linq.Expressions;
using System.Reflection;

namespace REST_Parser.ExpressionGenerators
{
    public class GuidExpressionGenerator<T> : IGuidExpressionGenerator<T>
    {
        public Expression<Func<T, bool>> GetExpression(string restOperator, ParameterExpression parameter, string field, string value)
        {
            try
            {
                var v = Guid.Parse(value);
                Type paramType = Expression.PropertyOrField(parameter, field).Type;
                MemberExpression member = Expression.PropertyOrField(parameter, field);
                ConstantExpression constantExpression = Expression.Constant(v);
                var conversion = Expression.Convert(constantExpression, paramType);

                switch (restOperator)
                {
                    case "eq":
                        return Expression.Lambda<Func<T, bool>>( Expression.Equal(member, conversion), parameter);
                    case "ne":
                        return Expression.Lambda<Func<T, bool>>( Expression.NotEqual(member, conversion), parameter);
                    default:
                        throw new REST_InvalidOperatorException(field, restOperator);
                }
            }
            catch(REST_InvalidOperatorException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
                throw new REST_InvalidFieldnameException(string.Format("field={0} value={1}", field, value));
            }
        }

    }
}
