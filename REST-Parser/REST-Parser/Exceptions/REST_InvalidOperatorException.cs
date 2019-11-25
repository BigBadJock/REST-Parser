using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser.Exceptions
{
    public class REST_InvalidOperatorException : Exception
    {
        public REST_InvalidOperatorException()
        {
        }

        public REST_InvalidOperatorException(string fieldname, string op) : base(string.Format("The REST request contained an invalid operator ({1}) for field ({0})", fieldname, op))
        {
        }
    }
}