using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators.Interfaces;
using REST_Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace REST_Parser
{
    public class RestToLinqParser<T> : IRestToLinqParser<T>
    {
        List<Expression<Func<T, bool>>> expressions = new List<Expression<Func<T, bool>>>();
        private IStringExpressionGenerator<T> stringExpressionGenerator;
        private IIntExpressionGenerator<T> intExpressionGenerator;
        private IDateExpressionGenerator<T> dateExpressionGenerator;
        private IDoubleExpressionGenerator<T> doubleExpressionGenerator;
        private IDecimalExpressionGenerator<T> decimalExpressionGenerator;
        private IBooleanExpressionGenerator<T> booleanExpressionGenerator;

        public RestToLinqParser(IStringExpressionGenerator<T> stringExpressionGenerator, IIntExpressionGenerator<T> intExpressionGenerator, IDateExpressionGenerator<T> dateExpressionGenerator, IDoubleExpressionGenerator<T> doubleExpressionGenerator, IDecimalExpressionGenerator<T> decimalExpressionGenerator, IBooleanExpressionGenerator<T> booleanExpressionGenerator)
        {
            this.stringExpressionGenerator = stringExpressionGenerator;
            this.intExpressionGenerator = intExpressionGenerator;
            this.dateExpressionGenerator = dateExpressionGenerator;
            this.doubleExpressionGenerator = doubleExpressionGenerator;
            this.decimalExpressionGenerator = decimalExpressionGenerator;
            this.booleanExpressionGenerator = booleanExpressionGenerator;
        }

        public RestResult<T> Parse(string request)
        {
            RestResult<T> result = new RestResult<T>();
            List<Expression<Func<T, bool>>> linqConditions = new List<Expression<Func<T, bool>>>();
            List<SortBy<T>> sortOrder = new List<SortBy<T>>();
            string[] conditions = GetConditions(request);
            foreach (string condition in conditions)
            {
                if (isSortCondition(condition))
                {
                    sortOrder.Add(parseSortCondition(condition));
                }
                else if (isPageCondition(condition))
                {
                    result = parsePageCondition(result, condition);
                }
                else
                {
                    linqConditions.Add(parseCondition(condition));
                }
            }
            result.Expressions = linqConditions;
            result.SortOrder = sortOrder;
            return result;
        }

        private bool isPageCondition(string condition)
        {
            return condition.Contains("$page");
        }

        private RestResult<T> parsePageCondition(RestResult<T> result, string condition)
        {
            string p = string.Empty;
            string value = string.Empty;

            GetCondition(condition, out p, out var ignore, out value);

            if(p.ToUpper() == "$PAGE")
            {
                result.Page = int.Parse(value);
            }
            if(p.ToUpper() == "$PAGESIZE")
            {
                result.PageSize = int.Parse(value);
            }
            return result;

        }

        private SortBy<T> parseSortCondition(string condition)
        {
            string sort = string.Empty;
            string sortOrder = string.Empty;
            string field = string.Empty;
            ParameterExpression parameter;

            parameter = Expression.Parameter(typeof(T), "p");
            GetCondition(condition, out sort, out sortOrder, out field);

            var expression = Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Property(parameter, field), typeof(object)), parameter);

            return new SortBy<T> { Expression = expression, Ascending = (sortOrder.ToUpper() == "ASC") };
        }

        private bool isSortCondition(string condition)
        {
            return condition.Contains("$sort_by");
        }

        protected internal string[] GetConditions(string request)
        {
            string[] conditions;
            conditions = request.Split('&');
            return conditions;
        }

        private Expression<Func<T, bool>> parseCondition(string condition)
        {
            string field = string.Empty;
            string value = string.Empty;
            string restOperator = string.Empty;
            ParameterExpression parameter;
            Type paramType;
            try
            {
                parameter = Expression.Parameter(typeof(T), "p");
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
            restOperator = "";
            if (sides[0].Contains("["))
            {
                field = (sides[0].Substring(0, sides[0].IndexOf("["))).Trim();
                restOperator = ExtractOperator(sides[0]);
            }
            else
            {
                field = sides[0];
            }
            value = sides[1].Trim();
        }
        
        public RestResult<T> Run(IQueryable<T> source, string rest)
        {
            RestResult<T> restResult = this.Parse(rest);

            IQueryable<T> selectedData = source;
            IOrderedQueryable<T> orderedData = null;
            restResult.Expressions.ForEach(delegate (Expression<Func<T, bool>> where) {
                selectedData = selectedData.Where(where);
            });
            bool firstSort = true;
            restResult.SortOrder.ForEach(delegate (SortBy<T> sortBy)
            {
                if (firstSort)
                {
                    if (sortBy.Ascending)
                    {
                        orderedData = selectedData.OrderBy<T, object>(sortBy.Expression);
                    }
                    else
                    {
                        orderedData = selectedData.OrderByDescending<T, object>(sortBy.Expression);
                    }
                    firstSort = false;
                }
                else
                {
                    if (sortBy.Ascending)
                    {
                        orderedData = orderedData.ThenBy<T, object>(sortBy.Expression);
                    }
                    else
                    {
                        orderedData = orderedData.ThenByDescending<T, object>(sortBy.Expression);
                    }
                }

            });
            if (!firstSort)
            {

                if(restResult.Page > 0 || restResult.PageSize > 0)
                {
                    restResult.Page = restResult.Page == 0 ? 1 : restResult.Page;
                    restResult.PageSize = restResult.PageSize == 0 ? 25 : restResult.PageSize;

                    var totalCount = orderedData.Count();
                    var pageCount = totalCount / restResult.PageSize;
                    if (pageCount * restResult.PageSize < totalCount) pageCount += 1;
                    if (restResult.Page > pageCount) restResult.Page = pageCount;
                    restResult.PageCount = pageCount;
                    restResult.TotalCount = totalCount;

                    orderedData = (IOrderedQueryable<T>)orderedData.Skip(restResult.PageSize * (restResult.Page - 1)).Take(restResult.PageSize);
                }

                restResult.Data = orderedData;
            }
            else
            {
                restResult.Data = selectedData;
            }

            return restResult;
        }


    }
}
