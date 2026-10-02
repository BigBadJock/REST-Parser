using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Globalization;
using System.Linq.Expressions;

namespace REST_Parser.ExpressionGenerators
{
    /// <summary>
    /// Handles the numeric types without a dedicated generator: long, float, short, byte, sbyte, ushort, uint and ulong.
    /// </summary>
    public class NumericExpressionGenerator<T> : INumericExpressionGenerator<T>
    {
        public Expression<Func<T, bool>> GetExpression(string restOperator, ParameterExpression parameter, string field, string value)
        {
            try
            {
                MemberExpression member = Expression.PropertyOrField(parameter, field);
                Type paramType = member.Type;
                Type numericType = Nullable.GetUnderlyingType(paramType) ?? paramType;
                // throws OverflowException for out-of-range values (e.g. 300 for a byte)
                object v = Convert.ChangeType(value, numericType, CultureInfo.InvariantCulture);
                ConstantExpression constantExpression = Expression.Constant(v, numericType);
                var conversion = Expression.Convert(constantExpression, paramType);

                switch (restOperator)
                {
                    case "eq":
                        return Expression.Lambda<Func<T, bool>>(Expression.Equal(member, conversion), parameter);
                    case "ne":
                        return Expression.Lambda<Func<T, bool>>(Expression.NotEqual(member, conversion), parameter);
                    case "gt":
                        return Expression.Lambda<Func<T, bool>>(Expression.GreaterThan(member, conversion), parameter);
                    case "ge":
                        return Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(member, conversion), parameter);
                    case "lt":
                        return Expression.Lambda<Func<T, bool>>(Expression.LessThan(member, conversion), parameter);
                    case "le":
                        return Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(member, conversion), parameter);
                    default:
                        throw new REST_InvalidOperatorException(field, restOperator);
                }
            }
            catch (REST_InvalidOperatorException)
            {
                throw;
            }
            catch (Exception ex)
            {
                throw new REST_InvalidValueException(field, value, ex);
            }
        }
    }
}
