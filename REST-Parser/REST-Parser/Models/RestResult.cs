using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace REST_Parser.Models
{
    /// <summary>
    /// Contains the result of parsing a REST query, including expressions, sort orders, data, and pagination metadata.
    /// </summary>
    /// <typeparam name="T">The entity type being queried.</typeparam>
    public class RestResult<T>
    {
        /// <summary>
        /// Gets or sets the list of filter expressions parsed from the REST query.
        /// </summary>
        public List<Expression<Func<T, bool>>> Expressions { get; set; } = new List<Expression<Func<T, bool>>>();

        /// <summary>
        /// Gets or sets the list of sort operations parsed from the REST query.
        /// </summary>
        public List<SortBy<T>> SortOrder { get; set; } = new List<SortBy<T>>();

        /// <summary>
        /// Gets or sets the IQueryable data result after applying filters, sorting, and pagination.
        /// This is populated by the <see cref="IRestToLinqParser{T}.Run"/> method.
        /// </summary>
        public IQueryable<T> Data { get; set; }

        /// <summary>
        /// Gets or sets the current page number (1-based). Default is 0 (no pagination).
        /// </summary>
        public int Page { get; set; }

        /// <summary>
        /// Gets or sets the number of items per page. Default is 0 (no pagination).
        /// Maximum value is enforced by the parser.
        /// </summary>
        public int PageSize { get; set; }

        /// <summary>
        /// Gets or sets the total number of pages based on <see cref="TotalCount"/> and <see cref="PageSize"/>.
        /// This is populated by the <see cref="IRestToLinqParser{T}.Run"/> method.
        /// </summary>
        public int PageCount { get; set; }

        /// <summary>
        /// Gets or sets the total number of items matching the filters (before pagination).
        /// This is populated by the <see cref="IRestToLinqParser{T}.Run"/> method.
        /// </summary>
        public int TotalCount { get; set; }


    }
}
