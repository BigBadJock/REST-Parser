using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser.Models
{
    public class RestResult<T>
    {
        public List<Expression<Func<T, bool>>> Expressions { get; set; }
        public List<SortBy<T>> SortOrder { get; set; }
        public IQueryable<T> Data { get; set; }
        public int Page { get; set; }
        public int PageSize { get; set; }
        public int PageCount { get; set; }
        public int TotalCount { get; set; }


    }
}
