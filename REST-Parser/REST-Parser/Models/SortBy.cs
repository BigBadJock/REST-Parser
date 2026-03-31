using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace REST_Parser.Models
{
    /// <summary>
    /// Represents a sort operation for a query result.
    /// </summary>
    /// <typeparam name="T">The entity type being sorted.</typeparam>
    public class SortBy<T>
    {
        /// <summary>
        /// Gets or sets the expression that selects the field to sort by.
        /// </summary>
        public Expression<Func<T, object>> Expression { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the sort is ascending (true) or descending (false).
        /// </summary>
        public bool Ascending { get; set; }
    }
}
