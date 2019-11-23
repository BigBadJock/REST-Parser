using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser
{
    public class RestToLinqParser<DataClassType>: BaseParser<List<Expression<Func<DataClassType, bool>>>>, IRestParser<List<Expression<Func<DataClassType, bool>>>>
    {
        List<Expression<Func<DataClassType, bool>>> expressions = new List<Expression<Func<DataClassType, bool>>>();

        public override List<Expression<Func<DataClassType, bool>>> Parse(string request)
        {
            List<Expression<Func<DataClassType, bool>>> linqConditions = new List<Expression<Func<DataClassType, bool>>>();
            string[] conditions = GetConditions(request);
            foreach(string condition in conditions)
            {
                linqConditions.Add(parseCondition(condition));
            }
            return linqConditions;
        }

        private Expression<Func<DataClassType, bool>> parseCondition(string condition)
        {
            // surname[eq] = McArthur

            var parameter = Expression.Parameter(typeof(DataClassType), "p");
            GetCondition(condition, out string field, out string restOperator, out string value);

            var paramType = Expression.PropertyOrField(parameter, field).Type;

            switch (Type.GetTypeCode(paramType))
            {
                case TypeCode.String:
                    return GetStringExpression(restOperator, parameter, field, value);
                case TypeCode.Int32:
                    return GetIntExpression(restOperator, parameter, field, value);
                default:
                    break;
            }

            return null;
        }

        private Expression<Func<DataClassType, bool>> GetIntExpression(string restOperator, ParameterExpression parameter, string field, string value)
        {
            int.TryParse(value, out int v);

            switch (restOperator)
            {
                case "eq":
                    return Expression.Lambda<Func<DataClassType, bool>>(
                        Expression.Equal(Expression.PropertyOrField(parameter, field), Expression.Constant(v)),
                        parameter);
                default:
                    return null;

            }
        }

        private Expression<Func<DataClassType, bool>> GetStringExpression(string restOperator, ParameterExpression parameter, string field, string value)
        {
            switch (restOperator)
            {
                case "eq":
                    return Expression.Lambda<Func<DataClassType, bool>>(
                        Expression.Equal(Expression.PropertyOrField(parameter, field), Expression.Constant(value)),
                        parameter);
                default:
                    return null;
            }
        }

        protected override internal string GetValue(string value)
        {
            string sqlValue;
            if (int.TryParse(value, out _))
            {
                sqlValue = value;
            }
            if (double.TryParse(value, out double d))
            {
                sqlValue = value;
            }
            else
            {
                sqlValue = value;
            }
            return sqlValue;
        }

    }
}
