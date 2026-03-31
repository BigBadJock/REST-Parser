using REST_Parser.Models;
using System.Linq;

namespace REST_Parser
{
    /// <summary>
    /// Parses REST query strings and converts them into LINQ expressions for dynamic querying.
    /// </summary>
    /// <typeparam name="T">The entity type to query against.</typeparam>
    public interface IRestToLinqParser<T>
    {
        /// <summary>
        /// Parses a REST query string into expression and sort components without executing against a data source.
        /// </summary>
        /// <param name="request">The REST query string (e.g., "name=John&amp;age[gt]=25&amp;$sort_by=name[ASC]&amp;$page=1&amp;$pagesize=10").</param>
        /// <returns>A <see cref="RestResult{T}"/> containing parsed expressions, sort orders, and pagination settings.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the query exceeds maximum length or condition count.</exception>
        /// <exception cref="REST_InvalidFieldnameException">Thrown when a field name in the query doesn't exist on the entity type.</exception>
        /// <exception cref="REST_InvalidOperatorException">Thrown when an invalid operator is used for a field type.</exception>
        /// <exception cref="REST_InvalidValueException">Thrown when a value cannot be converted to the field's type.</exception>
        RestResult<T> Parse(string request);

        /// <summary>
        /// Parses a REST query string and executes it against an IQueryable data source.
        /// </summary>
        /// <param name="source">The IQueryable data source to query (e.g., DbContext.Products).</param>
        /// <param name="rest">The REST query string (e.g., "name=John&amp;age[gt]=25&amp;$sort_by=name[ASC]&amp;$page=1&amp;$pagesize=10").</param>
        /// <returns>A <see cref="RestResult{T}"/> containing the filtered, sorted, and paginated data along with metadata.</returns>
        /// <exception cref="System.ArgumentException">Thrown when the query exceeds maximum length or condition count.</exception>
        /// <exception cref="REST_InvalidFieldnameException">Thrown when a field name in the query doesn't exist on the entity type.</exception>
        /// <exception cref="REST_InvalidOperatorException">Thrown when an invalid operator is used for a field type.</exception>
        /// <exception cref="REST_InvalidValueException">Thrown when a value cannot be converted to the field's type.</exception>
        RestResult<T> Run(IQueryable<T> source, string rest);
    }
}
