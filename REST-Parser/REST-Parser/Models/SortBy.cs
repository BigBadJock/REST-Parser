using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace REST_Parser.Models
{
    public class SortBy<T>
    {
        public Expression<Func<T, object>> Expression { get; set; }
        public bool Ascending { get; set; }
    }
}
