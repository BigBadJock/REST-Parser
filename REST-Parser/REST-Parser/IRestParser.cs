using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser
{
    public interface IRestParser<T>
    {
        T Parse(string request); 
    }
}
