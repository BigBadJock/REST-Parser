using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser
{
    public abstract class BaseParser<T> : IRestParser<T>
    {
        public abstract T Parse(string request);

        protected internal  string[] GetConditions(string request)
        {
            string[] conditions;
            conditions = request.Split('&');
            return conditions;
        }


        protected internal  string ExtractOperator(string query)
        {
            int start = query.IndexOf("[") + 1;
            int end = query.IndexOf("]");
            int length = end - start;
            string op = query.Substring(start, length).Trim();
            return op;
        }

        protected internal  void GetCondition(string condition, out string field, out string restOperator, out string value)
        {
            string[] sides = condition.Split('=');
            field = (sides[0].Substring(0, sides[0].IndexOf("["))).Trim();
            restOperator = ExtractOperator(sides[0]);
            value = sides[1].Trim();
        }


    }
}
