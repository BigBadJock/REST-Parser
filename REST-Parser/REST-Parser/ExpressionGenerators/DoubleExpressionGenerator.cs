using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser.ExpressionGenerators
{
    public class DoubleExpressionGenerator<T> : IDoubleExpressionGenerator<T>
    {
        public Expression<Func<T, bool>> GetExpression(string restOperator, ParameterExpression parameter, string field, string value)
        {
            try
            {
                double v = double.Parse(value);
                Type paramType = Expression.PropertyOrField(parameter, field).Type;
                MemberExpression member = Expression.PropertyOrField(parameter, field);
                ConstantExpression constantExpression = Expression.Constant(v);
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
