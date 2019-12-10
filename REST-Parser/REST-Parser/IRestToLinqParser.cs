using REST_Parser.Models;
using System.Linq;

namespace REST_Parser
{
    public interface IRestToLinqParser<T>
    {
        RestResult<T> Parse(string request);

        RestResult<T> Run(IQueryable<T> source, string rest);
    }
}
