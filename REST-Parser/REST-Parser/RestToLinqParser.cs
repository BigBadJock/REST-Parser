using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser
{
    public class RestToLinqParser<T>  : BaseParser, IRestParser<List<Expression<Func<T, bool>>>>
    {
        List<Expression<Func<T, bool>>> expressions = new List<Expression<Func<T, bool>>>();

        public override Object Parse(string request)
        {
            List<Expression<Func<T, bool>>> linqConditions = new List<Expression<Func<T, bool>>>();
            string[] conditions = GetConditions(request);
            foreach(string condition in conditions)
            {
                linqConditions.Add(parseCondition(condition));
            }
            return linqConditions;
        }

        private Expression<Func<T, bool>> parseCondition(string condition)
        {
            // surname[eq] = McArthur

            var parameter = Expression.Parameter(typeof(T), "p");
            GetCondition(condition, out string field, out string restOperator, out string value);
            switch (restOperator)
            {
                case "eq":
                    return Expression.Lambda<Func<T, bool>>(
                        Expression.Equal(Expression.PropertyOrField(parameter, field), Expression.Constant(value)),
                        parameter);
                default:
                    break;
            }

            return null;


            //List<Expression<Func<EventRuleViewDTO, bool>>> expressions = new List<Expression<Func<EventRuleViewDTO, bool>>>();
            //if (parameters.EventTypeId > 0) expressions.Add(a => a.EventTypeId == parameters.EventTypeId);
            //if (parameters.NotificationTypeId > 0) expressions.Add(a => a.NotificationTypeId == parameters.NotificationTypeId);
            //if (parameters.Active.HasValue) expressions.Add(a => a.Active == parameters.Active);
            //queryable = viewRepository.GetMany(expressions, pagination);

        }

    }
}
