using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace REST_Parser
{
    public class RestToLinqParser<DataClassType> : IRestParser<List<Expression<Func<DataClassType, bool>>>>
    {
        List<Expression<Func<DataClassType, bool>>> expressions = new List<Expression<Func<DataClassType, bool>>>();
        private IStringExpressionGenerator<DataClassType> stringExpressionGenerator;
        private IIntExpressionGenerator<DataClassType> intExpressionGenerator;
        private IDateExpressionGenerator<DataClassType> dateExpressionGenerator;
        private IDoubleExpressionGenerator<DataClassType> doubleExpressionGenerator;
        private IDecimalExpressionGenerator<DataClassType> decimalExpressionGenerator;
        private IBooleanExpressionGenerator<DataClassType> booleanExpressionGenerator;

        public RestToLinqParser(IStringExpressionGenerator<DataClassType> stringExpressionGenerator, IIntExpressionGenerator<DataClassType> intExpressionGenerator, IDateExpressionGenerator<DataClassType> dateExpressionGenerator, IDoubleExpressionGenerator<DataClassType> doubleExpressionGenerator, IDecimalExpressionGenerator<DataClassType> decimalExpressionGenerator, IBooleanExpressionGenerator<DataClassType> booleanExpressionGenerator)
        {
            this.stringExpressionGenerator = stringExpressionGenerator;
            this.intExpressionGenerator = intExpressionGenerator;
            this.dateExpressionGenerator = dateExpressionGenerator;
            this.doubleExpressionGenerator = doubleExpressionGenerator;
            this.decimalExpressionGenerator = decimalExpressionGenerator;
            this.booleanExpressionGenerator = booleanExpressionGenerator;
        }

        public List<Expression<Func<DataClassType, bool>>> Parse(string request)
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
            ParameterExpression parameter;
            Type paramType;
            try
            {
                parameter = Expression.Parameter(typeof(DataClassType), "p");
                GetCondition(condition, out field, out restOperator, out value);
                paramType = Expression.PropertyOrField(parameter, field).Type;
            }
            catch (Exception)
            {
                throw new REST_InvalidFieldnameException(field);

            }

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
                        return this.decimalExpressionGenerator.GetExpression(restOperator, parameter, field, value);
                    case TypeCode.Boolean:
                        return this.booleanExpressionGenerator.GetExpression(restOperator, parameter, field, value);
                    default:
                        break;
                }

                return null;
 


        }

        protected internal string[] GetConditions(string request)
        {
            string[] conditions;
            conditions = request.Split('&');
            return conditions;
        }


        protected internal string ExtractOperator(string query)
        {
            int start = query.IndexOf("[") + 1;
            int end = query.IndexOf("]");
            int length = end - start;
            string op = query.Substring(start, length).Trim();
            return op;
        }

        protected internal void GetCondition(string condition, out string field, out string restOperator, out string value)
        {
            string[] sides = condition.Split('=');
            field = (sides[0].Substring(0, sides[0].IndexOf("["))).Trim();
            restOperator = ExtractOperator(sides[0]);
            value = sides[1].Trim();
        }


    }
}
