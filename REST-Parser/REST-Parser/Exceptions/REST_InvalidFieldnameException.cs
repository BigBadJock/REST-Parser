using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser.Exceptions
{
    [Serializable]
    public class REST_InvalidFieldnameException: Exception
    {
        public REST_InvalidFieldnameException()
        {
        }

        public REST_InvalidFieldnameException(string fieldname) : base(string.Format("The REST request contained an invalid field name: {0}", fieldname))
        {

        }
    }
}
