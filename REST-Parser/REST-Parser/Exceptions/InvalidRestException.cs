using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser.Exceptions
{
    [Serializable]
    public class InvalidRestException: Exception
    {
        public InvalidRestException()
        {
        }

        public InvalidRestException(string message) : base(string.Format("The REST request was invalid: {0}", message))
        {

        }
    }
}
