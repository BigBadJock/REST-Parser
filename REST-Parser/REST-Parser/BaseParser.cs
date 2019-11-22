using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser
{
    public abstract class BaseParser : IRestParser<Object>
    {
        public abstract Object Parse(string request);

        protected internal static string[] GetConditions(string request)
        {
            string[] conditions;
            conditions = request.Split('&');
            return conditions;
        }


        protected internal static string ExtractOperator(string query)
        {
            int start = query.IndexOf("[") + 1;
            int end = query.IndexOf("]");
            int length = end - start;
            string op = query.Substring(start, length);
            return op;
        }

        protected internal static void GetCondition(string condition, out string field, out string restOperator, out string value)
        {
            string[] sides = condition.Split('=');
            field = sides[0].Substring(0, sides[0].IndexOf("["));
            restOperator = ExtractOperator(sides[0]);
            value = GetValue(sides[1]);
        }

        protected internal static string GetValue(string value)
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
                sqlValue = $"'{value}'";
            }
            return sqlValue;
        }


    }
}
