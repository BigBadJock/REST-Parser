using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace REST_Parser
{
    public class RestToLinqParser<DataClassType> : BaseParser<List<Expression<Func<DataClassType, bool>>>>, IRestParser<List<Expression<Func<DataClassType, bool>>>>
    {
        List<Expression<Func<DataClassType, bool>>> expressions = new List<Expression<Func<DataClassType, bool>>>();
        private IStringExpressionGenerator<DataClassType> stringExpressionGenerator;
        private IIntExpressionGenerator<DataClassType> intExpressionGenerator;
        private IDateExpressionGenerator<DataClassType> dateExpressionGenerator;
        private IDoubleExpressionGenerator<DataClassType> doubleExpressionGenerator;

        public RestToLinqParser(IStringExpressionGenerator<DataClassType> stringExpressionGenerator, IIntExpressionGenerator<DataClassType> intExpressionGenerator, IDateExpressionGenerator<DataClassType> dateExpressionGenerator, IDoubleExpressionGenerator<DataClassType> doubleExpressionGenerator)
        {
            this.stringExpressionGenerator = stringExpressionGenerator;
            this.intExpressionGenerator = intExpressionGenerator;
            this.dateExpressionGenerator = dateExpressionGenerator;
            this.doubleExpressionGenerator = doubleExpressionGenerator;
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
                        return this.dateExpressionGenerator.GetExpression(restOperator, parameter, field, value);
                    case TypeCode.Double:
                        return this.doubleExpressionGenerator.GetExpression(restOperator, parameter, field, value);
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


    }
}
