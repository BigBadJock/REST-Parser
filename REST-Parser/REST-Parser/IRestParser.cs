using REST_Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser
{
    public interface IRestParser<T>
    {
        RestResult<T> Parse(string request);

        IQueryable<T> Run(IQueryable<T> source, string rest);
    }
}
