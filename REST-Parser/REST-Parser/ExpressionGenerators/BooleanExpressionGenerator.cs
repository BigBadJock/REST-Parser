using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Linq.Expressions;

namespace REST_Parser.ExpressionGenerators
{
    public class BooleanExpressionGenerator<T> : IBooleanExpressionGenerator<T>
    {
        public Expression<Func<T, bool>> GetExpression(string restOperator, ParameterExpression parameter, string field, string value)
        {
            try
            {
                bool v = bool.Parse(value);

                var property = Expression.PropertyOrField(parameter, field);
                var propertyType = property.Type;
                Expression expression;

                if (propertyType == typeof(bool?))
                {
                    expression = Expression.Equal(property, Expression.Constant(v, typeof(bool?)));

                    switch (restOperator)
                    {
                        case "eq":
                            return Expression.Lambda<Func<T, bool>>(expression, parameter);
                        case "ne":
                            return Expression.Lambda<Func<T, bool>>(Expression.Not(expression), parameter);
                        default:
                            throw new REST_InvalidOperatorException(field, restOperator);
                    }
                }
                else if (propertyType == typeof(bool))
                {
                    expression = Expression.Equal(property, Expression.Constant(v));
                    switch (restOperator)
                    {
                        case "eq":
                            return Expression.Lambda<Func<T, bool>>(expression, parameter);
                        case "ne":
                            return Expression.Lambda<Func<T, bool>>(Expression.Not(expression), parameter);
                        default:
                            throw new REST_InvalidOperatorException(field, restOperator);
                    }
                }
                throw new Exception();
            }
            catch (REST_InvalidOperatorException ex)
            {
                throw ex;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                throw new REST_InvalidValueException(field, value);
            }
        }
    }
}
