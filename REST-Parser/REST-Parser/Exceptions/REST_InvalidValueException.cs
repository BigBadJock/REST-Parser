using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser.Exceptions
{
    public class REST_InvalidValueException : Exception
    {
        public REST_InvalidValueException()
        {
        }

        public REST_InvalidValueException(string fieldname, string value) : base(string.Format("The REST request contained an invalid value ({1}) for field ({0})", fieldname, value))
        {
        }
    }
}