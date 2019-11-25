using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser
{
    public class RestToLinqParser<DataClassType> : BaseParser<List<Expression<Func<DataClassType, bool>>>>, IRestParser<List<Expression<Func<DataClassType, bool>>>>
    {
        List<Expression<Func<DataClassType, bool>>> expressions = new List<Expression<Func<DataClassType, bool>>>();
        private IStringExpressionGenerator<DataClassType> stringExpressionGenerator;
        private IIntExpressionGenerator<DataClassType> intExpressionGenerator;


        public RestToLinqParser(IStringExpressionGenerator<DataClassType> stringExpressionGenerator, IIntExpressionGenerator<DataClassType> intExpressionGenerator)
        {
            this.stringExpressionGenerator = stringExpressionGenerator;
            this.intExpressionGenerator = intExpressionGenerator;
        }

        public override List<Expression<Func<DataClassType, bool>>> Parse(string request)
        {
            List<Expression<Func<DataClassType, bool>>> linqConditions = new List<Expression<Func<DataClassType, bool>>>();
            string[] conditions = GetConditions(request);
            foreach (string condition in conditions)
            {
                linqConditions.Add(parseCondition(condition));
            }
            return linqConditions;
        }

        private Expression<Func<DataClassType, bool>> parseCondition(string condition)
        {
            // surname[eq] = McArthur
            string field = string.Empty;
            string value = string.Empty;
            string restOperator = string.Empty;
            try
            {

                var parameter = Expression.Parameter(typeof(DataClassType), "p");
                GetCondition(condition, out field, out restOperator, out value);

                var paramType = Expression.PropertyOrField(parameter, field).Type;

                switch (Type.GetTypeCode(paramType))
                {
                    case TypeCode.String:
                        return this.stringExpressionGenerator.GetExpression(restOperator, parameter, field, value);
                    case TypeCode.Int32:
                        return this.intExpressionGenerator.GetExpression(restOperator, parameter, field, value);
                    case TypeCode.DateTime:
                        return GetDateTimeExpression(restOperator, parameter, field, value);
                    case TypeCode.Double:
                        return GetDoubleExpression(restOperator, parameter, field, value);
                    case TypeCode.Decimal:
                        return GetDecimalExpression(restOperator, parameter, field, value);

                    default:
                        break;
                }

                return null;
            }
            catch (Exception)
            {
                throw new InvalidRestException(string.Format("field={0} value={1}", field, value));

            }

        }

        private Expression<Func<DataClassType, bool>> GetDoubleExpression(string restOperator, ParameterExpression parameter, string field, string value)
        {
            try
            {

                double.TryParse(value, out double v);

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
            catch (Exception)
            {
                throw new InvalidRestException(string.Format("field={0} value={1}", field, value));
            }

        }

        private Expression<Func<DataClassType, bool>> GetDecimalExpression(string restOperator, ParameterExpression parameter, string field, string value)
        {
            try
            {

                decimal.TryParse(value, out decimal v);

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
            catch (Exception)
            {
                throw new InvalidRestException(string.Format("field={0} value={1}", field, value));
            }

        }

        private Expression<Func<DataClassType, bool>> GetDateTimeExpression(string restOperator, ParameterExpression parameter, string field, string value)
        {
            try
            {

                DateTime.TryParse(value, out DateTime d);

                switch (restOperator)
                {
                    case "eq":
                        return Expression.Lambda<Func<DataClassType, bool>>(
                            Expression.Equal(Expression.PropertyOrField(parameter, field), Expression.Constant(d)),
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
