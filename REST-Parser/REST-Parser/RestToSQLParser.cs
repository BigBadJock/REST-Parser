using System;
using System.Collections.Generic;
using System.Text;

namespace REST_Parser
{
    public class RestToSQLParser: BaseParser<string>, IRestParser<string>
    {
        public RestToSQLParser()
        {
        }

        public override string Parse(string request)
        {
            List<string> sqlConditions = new List<string>();
            string[] conditions = GetConditions(request);
            foreach (string condition in conditions)
            {
                sqlConditions.Add(parseCondition(condition));
            }
            StringBuilder sb = new StringBuilder();
            foreach (string sqlCondition in sqlConditions)
            {
                sb.Append(sqlCondition);
            }
            return sb.ToString();
        }

       private string parseCondition(string condition)
        {
            // surname[eq] = McArthur
            GetCondition(condition, out string field, out string restOperator, out string value);

            string sqlOp = GetSqlOperator(restOperator);
            string sql = string.Empty;

            sql = $"{field}{sqlOp}{value}";

            return sql;
        }

        private string GetSqlOperator(string op)
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


    }
}
