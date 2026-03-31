# REST-Parser Usage Guide

This guide provides detailed information on how to use the REST-Parser library in your .NET applications.

## Table of Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Query Syntax Reference](#query-syntax-reference)
- [Operators](#operators)
- [Filtering Examples](#filtering-examples)
- [Sorting Examples](#sorting-examples)
- [Pagination Examples](#pagination-examples)
- [Advanced Usage](#advanced-usage)
- [Exception Handling](#exception-handling)
- [API Reference](#api-reference)
- [Best Practices](#best-practices)
- [Troubleshooting](#troubleshooting)

---

## Installation

### NuGet Package Manager
```bash
Install-Package REST-Parser
```

### .NET CLI
```bash
dotnet add package REST-Parser
```

### Package Reference
```xml
<PackageReference Include="REST-Parser" Version="1.2.5" />
```

---

## Quick Start

### 1. Define Your Entity

```csharp
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public double? Rating { get; set; }
}
```

### 2. Register the Parser (Dependency Injection)

```csharp
using REST_Parser.DependencyResolution;

// In Program.cs or Startup.cs
builder.Services.RegisterRestParser<Product>();
```

### 3. Inject and Use in Your Service/Controller

```csharp
using REST_Parser;
using REST_Parser.Models;

public class ProductService
{
    private readonly IRestToLinqParser<Product> _parser;
    private readonly AppDbContext _context;

    public ProductService(IRestToLinqParser<Product> parser, AppDbContext context)
    {
        _parser = parser;
        _context = context;
    }

    public RestResult<Product> GetProducts(string query)
    {
        // Parse and execute the query
        return _parser.Run(_context.Products, query);
    }
}
```

### 4. Make a Request

```http
GET /api/products?category=Electronics&price[lt]=1000&$sort_by=price[ASC]&$page=1&$pagesize=20
```

---

## Query Syntax Reference

### Basic Format

```
field[operator]=value
```

### Multiple Conditions

Use `&` to separate conditions:

```
field1=value1&field2[operator]=value2&field3[operator]=value3
```

### Special Parameters

- **Sorting**: `$sort_by=field[ASC|DESC]`
- **Pagination**: `$page=n` and `$pagesize=n`

### Whitespace Handling

Whitespace is automatically trimmed:

```
field [eq] = value    ✅ Valid
field[eq]=value       ✅ Valid
field = value         ✅ Valid (defaults to eq)
```

---

## Operators

### Comparison Operators

| Operator | Description | Supported Types |
|----------|-------------|-----------------|
| `eq` | Equal to (default) | All types |
| `ne` | Not equal to | All types |
| `gt` | Greater than | int, double, decimal, DateTime |
| `ge` | Greater than or equal | int, double, decimal, DateTime |
| `lt` | Less than | int, double, decimal, DateTime |
| `le` | Less than or equal | int, double, decimal, DateTime |

### String Operators

| Operator | Description | Example |
|----------|-------------|---------|
| `eq` | Exact match (case-sensitive) | `name[eq]=iPhone` |
| `ne` | Not equal | `name[ne]=Samsung` |
| `contains` | Contains substring (case-sensitive) | `name[contains]=Pro` |

### Supported Data Types

- ✅ `string`
- ✅ `int` / `int?`
- ✅ `double` / `double?`
- ✅ `decimal` / `decimal?`
- ✅ `DateTime` / `DateTime?`
- ✅ `bool` / `bool?`
- ✅ `Guid` / `Guid?`

---

## Filtering Examples

### String Filtering

```csharp
// Exact match (default operator)
"name=iPhone"
"name[eq]=iPhone"

// Not equal
"category[ne]=Electronics"

// Contains (case-sensitive)
"description[contains]=wireless"

// Multiple string conditions
"category=Electronics&brand=Apple"
```

**REST Query:**
```http
GET /api/products?name[contains]=Pro&category=Electronics
```

### Numeric Filtering

```csharp
// Integer
"stock[gt]=10"           // Stock greater than 10
"stock[le]=100"          // Stock less than or equal to 100
"id=42"                  // ID equals 42 (default operator)

// Decimal/Double
"price[lt]=999.99"       // Price less than 999.99
"rating[ge]=4.5"         // Rating greater than or equal to 4.5
```

**REST Query:**
```http
GET /api/products?price[gt]=100&price[lt]=1000&stock[gt]=0
```

### Date Filtering

```csharp
// Standard date formats
"releaseDate[gt]=2023-01-01"
"releaseDate[lt]=2024-12-31"

// Date ranges
"releaseDate[ge]=2023-01-01&releaseDate[le]=2023-12-31"
```

**REST Query:**
```http
GET /api/products?releaseDate[gt]=2023-06-01&isActive=true
```

### Boolean Filtering

```csharp
// Boolean values
"isActive=true"
"isActive[eq]=false"
"isDiscontinued[ne]=true"
```

**REST Query:**
```http
GET /api/products?isActive=true&isFeatured=true
```

### GUID Filtering

```csharp
// GUID equality
"productId[eq]=123e4567-e89b-12d3-a456-426614174000"
"productId[ne]=123e4567-e89b-12d3-a456-426614174000"
```

### Nullable Field Filtering

```csharp
// Works with nullable types
"rating[ge]=4.0"         // double?
"discount[gt]=10"        // decimal?
"lastPurchaseDate[lt]=2024-01-01"  // DateTime?
```

---

## Sorting Examples

### Single Column Sort

```csharp
// Ascending (default)
"$sort_by=name"
"$sort_by=name[ASC]"

// Descending
"$sort_by=price[DESC]"
```

**REST Query:**
```http
GET /api/products?$sort_by=price[DESC]
```

### Multiple Column Sort

```csharp
// Sort by category ascending, then price descending
"$sort_by=category[ASC]&$sort_by=price[DESC]"

// Sort by rating descending, then name ascending
"$sort_by=rating[DESC]&$sort_by=name[ASC]"
```

**REST Query:**
```http
GET /api/products?category=Electronics&$sort_by=brand[ASC]&$sort_by=price[ASC]
```

### Default Sort Behavior

If no `$sort_by` is specified, results are automatically sorted by `Id` ascending:

```csharp
// These are equivalent
""
"$sort_by=Id[ASC]"
```

---

## Pagination Examples

### Basic Pagination

```csharp
// Get page 1 with 20 items
"$page=1&$pagesize=20"

// Get page 2 with 50 items
"$page=2&$pagesize=50"
```

**REST Query:**
```http
GET /api/products?$page=2&$pagesize=25
```

### Pagination with Filtering and Sorting

```csharp
// Complex query with all features
"category=Electronics&price[lt]=1000&$sort_by=price[ASC]&$page=1&$pagesize=20"
```

**REST Query:**
```http
GET /api/products?category=Electronics&isActive=true&$sort_by=name[ASC]&$page=1&$pagesize=10
```

### Default Pagination Behavior

- **Default Page**: 1 (if `$page` is specified without value)
- **Default Page Size**: 25 (if `$pagesize` is specified without value)
- **Maximum Page Size**: 1000 (enforced by the parser)

### Pagination Metadata

The `RestResult<T>` includes pagination information:

```csharp
var result = _parser.Run(_context.Products, query);

Console.WriteLine($"Page: {result.Page}");
Console.WriteLine($"Page Size: {result.PageSize}");
Console.WriteLine($"Total Count: {result.TotalCount}");
Console.WriteLine($"Total Pages: {result.PageCount}");

// Access the data
var products = result.Data.ToList();
```

---

## Advanced Usage

### Parse vs Run

#### Using `Parse()` - Just Parse, Don't Execute

```csharp
// Parse the query without executing it
var parseResult = _parser.Parse("category=Electronics&price[lt]=1000");

// Inspect what was parsed
Console.WriteLine($"Number of filters: {parseResult.Expressions.Count}");
Console.WriteLine($"Number of sorts: {parseResult.SortOrder.Count}");
Console.WriteLine($"Page: {parseResult.Page}, PageSize: {parseResult.PageSize}");

// Apply manually with custom logic
IQueryable<Product> query = _context.Products;

// Apply your own pre-filters
query = query.Where(p => p.IsActive);

// Apply parsed expressions
foreach (var expression in parseResult.Expressions)
{
    query = query.Where(expression);
}

// Apply sorting
var orderedQuery = query.OrderBy(parseResult.SortOrder[0].Expression);
// ... etc
```

#### Using `Run()` - Parse and Execute

```csharp
// Parse and execute in one call
var result = _parser.Run(_context.Products, "category=Electronics&price[lt]=1000");

// Data is already filtered, sorted, and paginated
var products = result.Data.ToList();
```

### Adding Custom Pre-Filters

```csharp
// Parse the user's query
var result = _parser.Parse(userQuery);

// Start with your base query
IQueryable<Product> query = _context.Products
    .Where(p => p.IsActive)           // Always filter active
    .Where(p => p.TenantId == tenantId); // Tenant isolation

// Apply user's filters
foreach (var expression in result.Expressions)
{
    query = query.Where(expression);
}

// Continue with sorting and pagination...
```

### Using with Entity Framework Core

```csharp
public async Task<RestResult<Product>> GetProductsAsync(string query)
{
    var result = _parser.Run(_context.Products.AsNoTracking(), query);
    
    // Materialize the query
    result.Data = result.Data.ToList().AsQueryable();
    
    return result;
}
```

### Projection for Performance

```csharp
var result = _parser.Run(_context.Products, query);

// Project to DTOs to reduce data transfer
var data = result.Data
    .Select(p => new ProductDto 
    { 
        Id = p.Id,
        Name = p.Name,
        Price = p.Price,
        Category = p.Category
    })
    .ToList();
```

### Multiple Entity Types

```csharp
// Register multiple parsers
builder.Services.RegisterRestParser<Product>();
builder.Services.RegisterRestParser<Customer>();
builder.Services.RegisterRestParser<Order>();

// Inject specific parsers
public class MultiService
{
    private readonly IRestToLinqParser<Product> _productParser;
    private readonly IRestToLinqParser<Customer> _customerParser;
    
    public MultiService(
        IRestToLinqParser<Product> productParser,
        IRestToLinqParser<Customer> customerParser)
    {
        _productParser = productParser;
        _customerParser = customerParser;
    }
}
```

---

## Exception Handling

### Exception Types

The library throws three custom exceptions:

```csharp
using REST_Parser.Exceptions;

try
{
    var result = _parser.Run(_context.Products, query);
    return Ok(result.Data.ToList());
}
catch (REST_InvalidFieldnameException ex)
{
    // Field doesn't exist on the entity
    // Example: "invalidField=value"
    return BadRequest(new { error = "Invalid field", details = ex.Message });
}
catch (REST_InvalidOperatorException ex)
{
    // Operator not supported for the field type
    // Example: "name[gt]=test" (gt not valid for strings)
    return BadRequest(new { error = "Invalid operator", details = ex.Message });
}
catch (REST_InvalidValueException ex)
{
    // Value cannot be converted to the field's type
    // Example: "price=notanumber"
    return BadRequest(new { error = "Invalid value", details = ex.Message });
}
catch (ArgumentException ex)
{
    // Security limits exceeded
    // - Query too long (>2000 chars)
    // - Too many conditions (>50)
    // - Invalid condition format
    return BadRequest(new { error = "Invalid query", details = ex.Message });
}
```

### Validation in API Controller

```csharp
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IRestToLinqParser<Product> _parser;
    private readonly AppDbContext _context;

    public ProductsController(IRestToLinqParser<Product> parser, AppDbContext context)
    {
        _parser = parser;
        _context = context;
    }

    [HttpGet]
    public IActionResult Get([FromQuery] string q)
    {
        // Provide default if empty
        if (string.IsNullOrWhiteSpace(q))
        {
            q = "$sort_by=Id&$page=1&$pagesize=20";
        }

        try
        {
            var result = _parser.Run(_context.Products, q);
            
            return Ok(new
            {
                data = result.Data.ToList(),
                pagination = new
                {
                    page = result.Page,
                    pageSize = result.PageSize,
                    totalCount = result.TotalCount,
                    totalPages = result.PageCount
                }
            });
        }
        catch (REST_InvalidFieldnameException ex)
        {
            return BadRequest(new { error = "Invalid field name", message = ex.Message });
        }
        catch (REST_InvalidOperatorException ex)
        {
            return BadRequest(new { error = "Invalid operator", message = ex.Message });
        }
        catch (REST_InvalidValueException ex)
        {
            return BadRequest(new { error = "Invalid value", message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { error = "Invalid query format", message = ex.Message });
        }
        catch (Exception ex)
        {
            // Log the exception
            return StatusCode(500, new { error = "An error occurred processing your request" });
        }
    }
}
```

---

## API Reference

### IRestToLinqParser<T>

#### Methods

**`RestResult<T> Parse(string request)`**

Parses a REST query string without executing it.

- **Parameters**: 
  - `request` - The REST query string
- **Returns**: `RestResult<T>` with parsed expressions and settings
- **Throws**: 
  - `ArgumentException` - Query exceeds limits
  - `REST_InvalidFieldnameException` - Invalid field name
  - `REST_InvalidOperatorException` - Invalid operator
  - `REST_InvalidValueException` - Invalid value

**`RestResult<T> Run(IQueryable<T> source, string rest)`**

Parses and executes a REST query against a data source.

- **Parameters**: 
  - `source` - The IQueryable data source
  - `rest` - The REST query string
- **Returns**: `RestResult<T>` with executed data and metadata
- **Throws**: Same as `Parse()`

### RestResult<T>

#### Properties

- **`List<Expression<Func<T, bool>>> Expressions`** - Filter expressions
- **`List<SortBy<T>> SortOrder`** - Sort operations
- **`IQueryable<T> Data`** - Query result (only populated by `Run()`)
- **`int Page`** - Current page number (1-based)
- **`int PageSize`** - Items per page
- **`int PageCount`** - Total number of pages
- **`int TotalCount`** - Total items matching filters

### SortBy<T>

#### Properties

- **`Expression<Func<T, object>> Expression`** - Sort expression
- **`bool Ascending`** - True for ascending, false for descending

---

## Best Practices

### 1. Always Validate Input

```csharp
[HttpGet]
public IActionResult Get([FromQuery] string q = "")
{
    if (string.IsNullOrWhiteSpace(q))
    {
        q = "$sort_by=Id&$pagesize=20"; // Sensible defaults
    }
    
    // Use try-catch for exception handling
    // ...
}
```

### 2. Enforce Maximum Page Size

```csharp
var result = _parser.Run(_context.Products, query);

// The parser already enforces MAX_PAGE_SIZE (1000)
// But you can add your own stricter limit
const int MAX_ALLOWED_PAGE_SIZE = 100;
if (result.PageSize > MAX_ALLOWED_PAGE_SIZE)
{
    return BadRequest($"Page size cannot exceed {MAX_ALLOWED_PAGE_SIZE}");
}
```

### 3. Use DTOs for API Responses

```csharp
var result = _parser.Run(_context.Products, query);

var response = new
{
    data = result.Data.Select(p => new ProductDto
    {
        Id = p.Id,
        Name = p.Name,
        Price = p.Price
    }).ToList(),
    page = result.Page,
    pageSize = result.PageSize,
    totalCount = result.TotalCount,
    totalPages = result.PageCount
};

return Ok(response);
```

### 4. Add Tenant/User Isolation

```csharp
// Parse the query
var parsed = _parser.Parse(query);

// Apply tenant filter first
IQueryable<Product> data = _context.Products
    .Where(p => p.TenantId == currentTenantId);

// Then apply user's filters
foreach (var expr in parsed.Expressions)
{
    data = data.Where(expr);
}
```

### 5. Use AsNoTracking for Read-Only Queries

```csharp
var result = _parser.Run(
    _context.Products.AsNoTracking(), 
    query
);
```

### 6. Log Failed Queries for Analysis

```csharp
catch (REST_InvalidFieldnameException ex)
{
    _logger.LogWarning(ex, "Invalid field in query: {Query}", query);
    return BadRequest(new { error = ex.Message });
}
```

### 7. Cache Common Queries

```csharp
// Use distributed cache for common queries
var cacheKey = $"products:{query}";
var cached = await _cache.GetStringAsync(cacheKey);

if (cached != null)
{
    return JsonSerializer.Deserialize<ProductListResponse>(cached);
}

var result = _parser.Run(_context.Products, query);
await _cache.SetStringAsync(cacheKey, 
    JsonSerializer.Serialize(result), 
    new DistributedCacheEntryOptions 
    { 
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5) 
    });
```

---

## Troubleshooting

### Common Issues

#### Issue: "Query exceeds maximum length"

**Cause**: Query string is longer than 2000 characters.

**Solution**: Simplify your query or contact support if you need a larger limit.

#### Issue: "Query exceeds maximum conditions"

**Cause**: More than 50 filter conditions in the query.

**Solution**: Reduce the number of conditions or use server-side filtering.

#### Issue: "Invalid field name"

**Cause**: Field doesn't exist on the entity.

**Solution**: Check spelling and ensure the property exists:
```csharp
public class Product 
{
    public int Id { get; set; }        // Use: id=1
    public string Name { get; set; }    // Use: name=iPhone
}
```

#### Issue: "Invalid operator"

**Cause**: Using an operator not supported for that type.

**Solution**: 
- Strings: Only `eq`, `ne`, `contains`
- Numbers/Dates: `eq`, `ne`, `gt`, `ge`, `lt`, `le`
- Booleans: Only `eq`, `ne`

#### Issue: "Invalid value"

**Cause**: Value cannot be converted to the field's type.

**Solution**: Ensure value matches the field type:
```csharp
price=999.99           // ✅ Correct for decimal
price=abc              // ❌ Invalid
releaseDate=2023-01-01 // ✅ Correct for DateTime
releaseDate=notadate   // ❌ Invalid
```

#### Issue: Case Sensitivity

**Cause**: String comparisons are case-sensitive.

**Solution**: 
```csharp
name=iPhone     // Matches "iPhone" but not "iphone"
name[contains]=pro  // Matches "MacBook Pro" but not "MacBook PRO"
```

If you need case-insensitive search, handle it on the server:
```csharp
var result = _parser.Parse(query);
IQueryable<Product> data = _context.Products;

// Apply case-insensitive filter manually
data = data.Where(p => p.Name.ToLower().Contains(searchTerm.ToLower()));

// Then apply other filters
foreach (var expr in result.Expressions)
{
    data = data.Where(expr);
}
```

#### Issue: No Results Returned

**Possible causes**:
1. Filters are too restrictive
2. Data doesn't exist
3. Tenant/user isolation filters

**Debug**:
```csharp
var result = _parser.Parse(query);
Console.WriteLine($"Filters: {result.Expressions.Count}");
Console.WriteLine($"Sort: {result.SortOrder.Count}");

// Test without filters
var allData = _context.Products.ToList();
Console.WriteLine($"Total records: {allData.Count}");
```

---

## Security Limits

The parser enforces the following limits to prevent abuse:

| Limit | Value | Description |
|-------|-------|-------------|
| MAX_QUERY_LENGTH | 2000 | Maximum query string length |
| MAX_CONDITIONS | 50 | Maximum number of filter conditions |
| MAX_PAGE_SIZE | 1000 | Maximum page size |

These limits are enforced automatically and will throw `ArgumentException` if exceeded.

---

## Complete Example

Here's a complete working example:

```csharp
// Entity
public class Product
{
    public int Id { get; set; }
    public string Name { get; set; }
    public string Category { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool IsActive { get; set; }
}

// Startup/Program.cs
builder.Services.AddDbContext<AppDbContext>();
builder.Services.RegisterRestParser<Product>();

// Controller
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IRestToLinqParser<Product> _parser;
    private readonly AppDbContext _context;
    private readonly ILogger<ProductsController> _logger;

    public ProductsController(
        IRestToLinqParser<Product> parser, 
        AppDbContext context,
        ILogger<ProductsController> logger)
    {
        _parser = parser;
        _context = context;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult Get([FromQuery] string q = "")
    {
        try
        {
            // Default query if none provided
            if (string.IsNullOrWhiteSpace(q))
            {
                q = "$sort_by=Id&$pagesize=20";
            }

            // Parse and execute
            var result = _parser.Run(_context.Products.AsNoTracking(), q);
            
            // Build response
            return Ok(new
            {
                data = result.Data.Select(p => new
                {
                    p.Id,
                    p.Name,
                    p.Category,
                    p.Price,
                    p.Stock
                }).ToList(),
                pagination = new
                {
                    page = result.Page,
                    pageSize = result.PageSize,
                    totalCount = result.TotalCount,
                    totalPages = result.PageCount
                }
            });
        }
        catch (REST_InvalidFieldnameException ex)
        {
            _logger.LogWarning(ex, "Invalid field in query: {Query}", q);
            return BadRequest(new { error = "Invalid field name", message = ex.Message });
        }
        catch (REST_InvalidOperatorException ex)
        {
            _logger.LogWarning(ex, "Invalid operator in query: {Query}", q);
            return BadRequest(new { error = "Invalid operator", message = ex.Message });
        }
        catch (REST_InvalidValueException ex)
        {
            _logger.LogWarning(ex, "Invalid value in query: {Query}", q);
            return BadRequest(new { error = "Invalid value", message = ex.Message });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning(ex, "Invalid query format: {Query}", q);
            return BadRequest(new { error = "Invalid query", message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing query: {Query}", q);
            return StatusCode(500, new { error = "An error occurred processing your request" });
        }
    }
}

// Sample Requests
// GET /api/products
// GET /api/products?category=Electronics
// GET /api/products?price[gt]=100&price[lt]=1000
// GET /api/products?name[contains]=Pro&isActive=true
// GET /api/products?category=Electronics&$sort_by=price[ASC]&$page=1&$pagesize=10
```

---

## Additional Resources

- **GitHub Repository**: https://github.com/BigBadJock/REST-Parser
- **NuGet Package**: https://www.nuget.org/packages/REST-Parser/
- **Report Issues**: https://github.com/BigBadJock/REST-Parser/issues

---

**Last Updated**: 2025  
**Library Version**: 1.2.5  
**Target Framework**: .NET 10
