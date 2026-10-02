using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Linq.Expressions;

namespace REST_Parser.ExpressionGenerators
{
    /// <summary>
    /// Handles enum fields. Values may be member names (case-insensitive) or underlying numeric values.
    /// </summary>
    public class EnumExpressionGenerator<T> : IEnumExpressionGenerator<T>
    {
        public Expression<Func<T, bool>> GetExpression(string restOperator, ParameterExpression parameter, string field, string value)
        {
            try
            {
                MemberExpression member = Expression.PropertyOrField(parameter, field);
                Type paramType = member.Type;
                bool isNullable = Nullable.GetUnderlyingType(paramType) != null;
                Type enumType = Nullable.GetUnderlyingType(paramType) ?? paramType;

                if (!Enum.TryParse(enumType, value, true, out object v))
                {
                    throw new FormatException($"'{value}' is not a valid {enumType.Name} value");
                }

                // eq/ne compare enums directly so value converters (e.g. enums stored as strings) still translate
                var enumConstant = Expression.Convert(Expression.Constant(v, enumType), paramType);

                // enums have no ordering operators, so gt/ge/lt/le compare the underlying numeric values
                Type underlyingType = Enum.GetUnderlyingType(enumType);
                Type comparisonType = isNullable ? typeof(Nullable<>).MakeGenericType(underlyingType) : underlyingType;
                var numericMember = Expression.Convert(member, comparisonType);
                var numericConstant = Expression.Convert(Expression.Constant(Convert.ChangeType(v, underlyingType), underlyingType), comparisonType);

                switch (restOperator)
                {
                    case "eq":
                        return Expression.Lambda<Func<T, bool>>(Expression.Equal(member, enumConstant), parameter);
                    case "ne":
                        return Expression.Lambda<Func<T, bool>>(Expression.NotEqual(member, enumConstant), parameter);
                    case "gt":
                        return Expression.Lambda<Func<T, bool>>(Expression.GreaterThan(numericMember, numericConstant), parameter);
                    case "ge":
                        return Expression.Lambda<Func<T, bool>>(Expression.GreaterThanOrEqual(numericMember, numericConstant), parameter);
                    case "lt":
                        return Expression.Lambda<Func<T, bool>>(Expression.LessThan(numericMember, numericConstant), parameter);
                    case "le":
                        return Expression.Lambda<Func<T, bool>>(Expression.LessThanOrEqual(numericMember, numericConstant), parameter);
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
