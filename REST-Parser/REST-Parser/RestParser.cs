using System;
using System.Collections.Generic;
using System.Text;

namespace REST_Parser
{
    public static class RestParser
    {
        public static string Parse(string request)
        {
            List<string> sqlConditions = new List<string>();
            string[] conditions = request.Split('&');
            foreach(string condition in conditions)
            {
                sqlConditions.Add(parseCondition(condition));
            }
            StringBuilder sb = new StringBuilder();
            foreach(string sqlCondition in sqlConditions)
            {
                sb.Append(sqlCondition);
            }
            return sb.ToString();
        }

        private static string parseCondition(string condition)
        {
            // surname[eq] = McArthur
            string[] sides = condition.Split('=');
            string field = sides[0].Substring(0, sides[0].IndexOf("["));
            string sqlOp = GetOperator(sides);
            string sql = string.Empty;

            string value = getValue(sides[1]);
            sql = $"{field}{sqlOp}{value}";

            return sql;
        }

        private static string getValue(string value)
        {
            string sqlValue;
            int i;
            double d;
            if (int.TryParse(value, out i))
            {
                sqlValue = value;
            } if(double.TryParse(value, out d))
            {
                sqlValue = value;
            }
            else
            {
                sqlValue = $"'{value}'";
            }
            return sqlValue;
        }

        private static string GetOperator(string[] sides)
        {
            string op = extractOperator(sides[0]);
            string sqlOp = GetSqlOperator(op);
            return sqlOp;
        }

        private static string GetSqlOperator(string op)
        {
            string sqlOp = "";
            switch (op)
            {
                case "eq":
                    sqlOp = " = ";
                    break;
                case "ne":
                    sqlOp = " <> ";
                    break;
                default:
                    sqlOp = " = ";
                    break;
            }

            return sqlOp;
        }

        private static string extractOperator(string query)
        {
            int start = query.IndexOf("[")+1;
            int end = query.IndexOf("]");
            int length = end - start;
            string op = query.Substring(start, length);
            return op;
        }
    }
}
