using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser.ExpressionGenerators.Interfaces
{
    public interface IExpressionGenerator<T>
    {
        Expression<Func<T, bool>> GetExpression(string restOperator, ParameterExpression parameter, string field, string value);
    }
}
