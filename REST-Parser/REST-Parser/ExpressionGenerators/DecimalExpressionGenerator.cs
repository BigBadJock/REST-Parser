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
    public class DecimalExpressionGenerator<T> : IDecimalExpressionGenerator<T>
    {
        public Expression<Func<T, bool>> GetExpression(string restOperator, ParameterExpression parameter, string field, string value)
        {
            try
            {
                decimal.TryParse(value, out decimal v);

                switch (restOperator)
                {
                    case "eq":
                        return Expression.Lambda<Func<T, bool>>(
                            Expression.Equal(Expression.PropertyOrField(parameter, field), Expression.Constant(v)),
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
