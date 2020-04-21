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
                Type paramType = Expression.PropertyOrField(parameter, field).Type;
                MemberExpression member = Expression.PropertyOrField(parameter, field);
                ConstantExpression constantExpression = Expression.Constant(value);
                UnaryExpression conversion = Expression.Convert(constantExpression, paramType);

                switch (restOperator)
                {
                    case "eq":
                        return Expression.Lambda<Func<T, bool>>( Expression.Equal(member, conversion), parameter);
                    case "ne":
                        return Expression.Lambda<Func<T, bool>>( Expression.NotEqual(member, conversion), parameter);
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
            Type paramType = property.Type;
            ConstantExpression constantExpression = Expression.Constant(value, paramType);
            UnaryExpression conversion = Expression.Convert(constantExpression, paramType);
            MethodInfo method = paramType.GetMethod("Contains", new[] { paramType });
            var containsMethodExp = Expression.Call(property, method, conversion);
            var nullCheck = Expression.NotEqual(property, Expression.Constant(null, typeof(object)));
            var combined = Expression.AndAlso(nullCheck, containsMethodExp);
            var containsExpression = Expression.Lambda<Func<T, bool>>(combined, parameter);
            return containsExpression;
        }
    }
}
