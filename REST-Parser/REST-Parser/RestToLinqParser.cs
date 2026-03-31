using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators.Interfaces;
using REST_Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace REST_Parser
{
    /// <summary>
    /// Implementation of <see cref="IRestToLinqParser{T}"/> that parses REST query strings and converts them into LINQ expressions.
    /// Supports filtering, sorting, and pagination with security limits.
    /// </summary>
    /// <typeparam name="T">The entity type to query against.</typeparam>
    public class RestToLinqParser<T> : IRestToLinqParser<T>
    {
        private const int MAX_PAGE_SIZE = 1000;
        private const int MAX_CONDITIONS = 50;
        private const int MAX_QUERY_LENGTH = 2000;
        private const string PAGE_PARAM = "$PAGE";
        private const string PAGESIZE_PARAM = "$PAGESIZE";
        private const string ASC_ORDER = "ASC";
        private const string DESC_ORDER = "DESC";

        private IStringExpressionGenerator<T> stringExpressionGenerator;
        private IIntExpressionGenerator<T> intExpressionGenerator;
        private IDateExpressionGenerator<T> dateExpressionGenerator;
        private IDoubleExpressionGenerator<T> doubleExpressionGenerator;
        private IDecimalExpressionGenerator<T> decimalExpressionGenerator;
        private IBooleanExpressionGenerator<T> booleanExpressionGenerator;
        private IGuidExpressionGenerator<T> guidExpressionGenerator;

        /// <summary>
        /// Initializes a new instance of the <see cref="RestToLinqParser{T}"/> class.
        /// </summary>
        /// <param name="stringExpressionGenerator">Generator for string field expressions.</param>
        /// <param name="intExpressionGenerator">Generator for integer field expressions.</param>
        /// <param name="dateExpressionGenerator">Generator for DateTime field expressions.</param>
        /// <param name="doubleExpressionGenerator">Generator for double field expressions.</param>
        /// <param name="decimalExpressionGenerator">Generator for decimal field expressions.</param>
        /// <param name="booleanExpressionGenerator">Generator for boolean field expressions.</param>
        /// <param name="guidExpressionGenerator">Generator for Guid field expressions.</param>
        public RestToLinqParser(IStringExpressionGenerator<T> stringExpressionGenerator, IIntExpressionGenerator<T> intExpressionGenerator, IDateExpressionGenerator<T> dateExpressionGenerator, IDoubleExpressionGenerator<T> doubleExpressionGenerator, IDecimalExpressionGenerator<T> decimalExpressionGenerator, IBooleanExpressionGenerator<T> booleanExpressionGenerator, IGuidExpressionGenerator<T> guidExpressionGenerator)
        {
            this.stringExpressionGenerator = stringExpressionGenerator;
            this.intExpressionGenerator = intExpressionGenerator;
            this.dateExpressionGenerator = dateExpressionGenerator;
            this.doubleExpressionGenerator = doubleExpressionGenerator;
            this.decimalExpressionGenerator = decimalExpressionGenerator;
            this.booleanExpressionGenerator = booleanExpressionGenerator;
            this.guidExpressionGenerator = guidExpressionGenerator;
        }

        /// <inheritdoc/>
        public RestResult<T> Parse(string request)
        {
            if (request != null && request.Length > MAX_QUERY_LENGTH)
            {
                throw new ArgumentException($"Query exceeds maximum length of {MAX_QUERY_LENGTH}");
            }

            RestResult<T> result = new RestResult<T>();
            if (!string.IsNullOrWhiteSpace(request))
            {
                List<Expression<Func<T, bool>>> linqConditions = new List<Expression<Func<T, bool>>>();
                List<SortBy<T>> sortOrder = new List<SortBy<T>>();
                string[] conditions = GetConditions(request);

                if (conditions.Length > MAX_CONDITIONS)
                {
                    throw new ArgumentException($"Query exceeds maximum of {MAX_CONDITIONS} conditions");
                }

                foreach (string condition in conditions)
                {
                    if (IsSortCondition(condition))
                    {
                        sortOrder.Add(ParseSortCondition(condition));
                    }
                    else if (IsPageCondition(condition))
                    {
                        result = ParsePageCondition(result, condition);
                    }
                    else
                    {
                        linqConditions.Add(ParseCondition(condition));
                    }
                }

                if (sortOrder.Count == 0)
                {
                    sortOrder.Add(ParseSortCondition("$sort_by=Id"));
                }

                result.Expressions = linqConditions;
                result.SortOrder = sortOrder;
            }
            else
            {
                List<SortBy<T>> sortOrder = new List<SortBy<T>>();
                sortOrder.Add(ParseSortCondition("$sort_by=Id"));
                result.SortOrder = sortOrder;

            }
            return result;
        }

        private bool IsPageCondition(string condition)
        {
            return condition.ToUpper().Contains("$PAGE");
        }

        private RestResult<T> ParsePageCondition(RestResult<T> result, string condition)
        {
            string p = string.Empty;
            string value = string.Empty;

            GetCondition(condition, out p, out var ignore, out value);

            string upperParam = p.ToUpper();
            if (upperParam == PAGE_PARAM)
            {
                if (!int.TryParse(value, out int pageValue) || pageValue < 1)
                {
                    throw new REST_InvalidValueException(p, value);
                }
                result.Page = pageValue;
            }
            else if (upperParam == PAGESIZE_PARAM)
            {
                if (!int.TryParse(value, out int pageSizeValue) || pageSizeValue < 1)
                {
                    throw new REST_InvalidValueException(p, value);
                }
                result.PageSize = pageSizeValue > MAX_PAGE_SIZE ? MAX_PAGE_SIZE : pageSizeValue;
            }
            return result;

        }

        private SortBy<T> ParseSortCondition(string condition)
        {
            string sort = string.Empty;
            string sortOrder = string.Empty;
            string field = string.Empty;
            ParameterExpression parameter;

            parameter = Expression.Parameter(typeof(T), "p");
            GetCondition(condition, out sort, out sortOrder, out field);

            if (string.IsNullOrWhiteSpace(sortOrder))
            {
                sortOrder = ASC_ORDER;
            }

            var expression = Expression.Lambda<Func<T, object>>(Expression.Convert(Expression.Property(parameter, field), typeof(object)), parameter);

            return new SortBy<T> { Expression = expression, Ascending = (sortOrder.ToUpper() == ASC_ORDER) };
        }

        private bool IsSortCondition(string condition)
        {
            return condition.ToUpper().Contains("$SORT_BY");
        }

        protected internal string[] GetConditions(string request)
        {
            string[] conditions;
            conditions = request.Split('&');
            return conditions;
        }

        private Expression<Func<T, bool>> ParseCondition(string condition)
        {
            if (string.IsNullOrWhiteSpace(condition)) return null;
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

                if (paramType.IsGenericType && paramType.GetGenericTypeDefinition() == typeof(Nullable<>))
                {
                    paramType = Nullable.GetUnderlyingType(paramType);
                }

                if (Nullable.GetUnderlyingType(paramType) != null)
                {
                    paramType = Nullable.GetUnderlyingType(paramType);
                }

            }
            catch (Exception ex)
            {
                throw new REST_InvalidFieldnameException(field, ex);

            }

            if (string.IsNullOrEmpty(restOperator))
            {
                restOperator = "eq";
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
                case TypeCode.Object:
                    if (paramType == typeof(Guid))
                    {
                        return this.guidExpressionGenerator.GetExpression(restOperator, parameter, field, value);
                    }
                    break;
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
            ArgumentNullException.ThrowIfNull(condition);

            string[] sides = condition.Split('=');
            if (sides.Length < 2)
            {
                throw new ArgumentException($"Invalid condition format: {condition}");
            }

            restOperator = "";
            if (sides[0].Contains("["))
            {
                field = (sides[0].Substring(0, sides[0].IndexOf("["))).Trim();
                restOperator = ExtractOperator(sides[0]);
            }
            else
            {
                field = sides[0].Trim();
            }
            value = sides[1].Trim();
        }

        /// <inheritdoc/>
        public RestResult<T> Run(IQueryable<T> source, string rest)
        {
            RestResult<T> restResult = this.Parse(rest);

            IQueryable<T> selectedData = source;
            IOrderedQueryable<T> orderedData = null;
            if (restResult.Expressions != null)
            {
                restResult.Expressions.ForEach(delegate (Expression<Func<T, bool>> where)
                {
                    if (where != null)
                        selectedData = selectedData.Where(where);
                });
            }
            bool firstSort = true;
            if (restResult.SortOrder != null)
            {
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
            }
            if (!firstSort)
            {

                if (restResult.Page > 0 || restResult.PageSize > 0)
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
