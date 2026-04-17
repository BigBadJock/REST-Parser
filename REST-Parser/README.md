# REST-Parser

[![NuGet](https://img.shields.io/nuget/v/REST-Parser.svg)](https://www.nuget.org/packages/REST-Parser/)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)

A powerful .NET library that converts REST query strings into LINQ expressions and executes them against `IQueryable` data sources. Perfect for building flexible APIs with dynamic filtering, sorting, and pagination.

## Features

✨ **Dynamic Filtering** - Support for equality, comparison, and contains operations  
📊 **Multi-column Sorting** - Ascending and descending sort with multiple fields  
📄 **Built-in Pagination** - Page-based navigation with automatic count calculation  
🎯 **Type-Safe** - Strongly-typed expression generation  
🔧 **Dependency Injection** - First-class DI support  
⚡ **High Performance** - Generates optimized LINQ expressions  
🛡️ **Nullable Support** - Handles nullable types seamlessly  

## Installation

```bash
dotnet add package REST-Parser
```

## Quick Start

### 1. Register the Parser

```csharp
using REST_Parser.DependencyResolution;

// In your Startup.cs or Program.cs
services.RegisterRestParser<YourEntity>();
```

### 2. Use in Your Code

```csharp
using REST_Parser;
using REST_Parser.Models;

public class ProductService
{
    private readonly IRestToLinqParser<Product> _parser;
    private readonly IQueryable<Product> _products;

    public ProductService(IRestToLinqParser<Product> parser, AppDbContext context)
    {
        _parser = parser;
        _products = context.Products;
    }

    public RestResult<Product> GetProducts(string query)
    {
        // Parse and execute the query in one call
        return _parser.Run(_products, query);
    }

    // Or parse separately for more control
    public IQueryable<Product> SearchProducts(string query)
    {
        var result = _parser.Parse(query);
        
        IQueryable<Product> filtered = _products;
        foreach (var expression in result.Expressions)
        {
            filtered = filtered.Where(expression);
        }
        
        return filtered;
    }
}
```

## Query Syntax

### Basic Filtering

Filter by field equality (default operator):
```
name=iPhone
```

### Operators

Use bracket notation for specific operations:

| Operator | Description | Example |
|----------|-------------|---------|
| `eq` | Equal to | `price[eq]=999` |
| `ne` | Not equal to | `category[ne]=Electronics` |
| `gt` | Greater than | `stock[gt]=10` |
| `ge` | Greater than or equal | `rating[ge]=4` |
| `lt` | Less than | `price[lt]=100` |
| `le` | Less than or equal | `discount[le]=20` |
| `contains` | String contains (case-sensitive) | `description[contains]=wireless` |

### Supported Types

- `string` - eq, ne, contains
- `int` - eq, ne, gt, ge, lt, le
- `double` - eq, ne, gt, ge, lt, le
- `decimal` - eq, ne, gt, ge, lt, le
- `DateTime` - eq, ne, gt, ge, lt, le
- `bool` - eq, ne
- `Guid` - eq, ne

All types support nullable variants (`int?`, `DateTime?`, etc.)

### Multiple Conditions

Combine multiple filters with `&`:
```
category=Electronics&price[lt]=1000&inStock[eq]=true
```

### Sorting

#### Single Column
```
$sort_by=name
$sort_by[ASC]=name
$sort_by[DESC]=price
```

#### Multiple Columns
```
category=Electronics&$sort_by[ASC]=brand&$sort_by[DESC]=price
```

**Note:** If no sort order is specified, results are sorted by `Id` ascending by default.

### Pagination

```
$page=2&$pagesize=25
```

- `$page` - Page number (1-based, defaults to 1)
- `$pagesize` - Items per page (defaults to 25)

The `RestResult<T>` includes pagination metadata:
- `PageCount` - Total number of pages
- `TotalCount` - Total number of items
- `Page` - Current page number
- `PageSize` - Items per page

## Complete Example

### Entity Class

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

### API Controller

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
    public IActionResult Get([FromQuery] string query)
    {
        var result = _parser.Run(_context.Products, query);
        
        return Ok(new
        {
            data = result.Data.ToList(),
            page = result.Page,
            pageSize = result.PageSize,
            totalCount = result.TotalCount,
            pageCount = result.PageCount
        });
    }
}
```

### Query Examples

```bash
# Get all active electronics under $500
GET /api/products?category=Electronics&isActive=true&price[lt]=500

# Search for "wireless" products, sorted by price
GET /api/products?description[contains]=wireless&$sort_by=price[ASC]

# Get page 2 of products with rating >= 4, sorted by rating then price
GET /api/products?rating[ge]=4&$sort_by=rating[DESC]&$sort_by=price[ASC]&$page=2&$pagesize=20

# Get all products released after 2023-01-01
GET /api/products?releaseDate[gt]=2023-01-01
```

## Advanced Usage

### Using Parse vs Run

**`Parse()`** - Returns parsed expressions without executing:
```csharp
RestResult<Product> result = _parser.Parse("category=Electronics&price[lt]=1000");

// result.Expressions - List of filter expressions
// result.SortOrder - List of sort operations
// result.Page, result.PageSize - Pagination settings
```

**`Run()`** - Parses and executes against a data source:
```csharp
RestResult<Product> result = _parser.Run(_context.Products, "category=Electronics&price[lt]=1000");

// result.Data - IQueryable with filters, sorting, and pagination applied
// result.TotalCount - Total items matching filters (before pagination)
// result.PageCount - Total number of pages
```

### Custom Expression Building

```csharp
var parseResult = _parser.Parse("price[gt]=100");

IQueryable<Product> query = _context.Products;

// Apply each filter expression
foreach (var expression in parseResult.Expressions)
{
    query = query.Where(expression);
}

// Apply custom logic here
query = query.Where(p => p.IsActive);

// Apply sorting
var orderedQuery = query.OrderBy(parseResult.SortOrder[0].Expression);
for (int i = 1; i < parseResult.SortOrder.Count; i++)
{
    var sort = parseResult.SortOrder[i];
    orderedQuery = sort.Ascending 
        ? orderedQuery.ThenBy(sort.Expression)
        : orderedQuery.ThenByDescending(sort.Expression);
}
```

## Exception Handling

The library throws specific exceptions for different error scenarios:

```csharp
try
{
    var result = _parser.Run(_context.Products, query);
}
catch (REST_InvalidFieldnameException ex)
{
    // Invalid field name in query
    // Example: "invalidField=value"
    return BadRequest($"Invalid field: {ex.Message}");
}
catch (REST_InvalidOperatorException ex)
{
    // Invalid operator for the field type
    // Example: "name[gt]=test" (gt not supported for strings)
    return BadRequest($"Invalid operator: {ex.Message}");
}
catch (REST_InvalidValueException ex)
{
    // Value cannot be converted to field type
    // Example: "price=notanumber"
    return BadRequest($"Invalid value: {ex.Message}");
}
```

## Dependency Injection Setup

### ASP.NET Core

```csharp
// Program.cs or Startup.cs
using REST_Parser.DependencyResolution;

var builder = WebApplication.CreateBuilder(args);

// Register parser for each entity type you need
builder.Services.RegisterRestParser<Product>();
builder.Services.RegisterRestParser<Customer>();
builder.Services.RegisterRestParser<Order>();

var app = builder.Build();
```

### Manual Registration (without extension method)

```csharp
using REST_Parser;
using REST_Parser.ExpressionGenerators;
using REST_Parser.ExpressionGenerators.Interfaces;

services.AddSingleton<IStringExpressionGenerator<Product>, StringExpressionGenerator<Product>>();
services.AddSingleton<IIntExpressionGenerator<Product>, IntExpressionGenerator<Product>>();
services.AddSingleton<IDateExpressionGenerator<Product>, DateExpressionGenerator<Product>>();
services.AddSingleton<IDoubleExpressionGenerator<Product>, DoubleExpressionGenerator<Product>>();
services.AddSingleton<IDecimalExpressionGenerator<Product>, DecimalExpressionGenerator<Product>>();
services.AddSingleton<IBooleanExpressionGenerator<Product>, BooleanExpressionGenerator<Product>>();
services.AddSingleton<IGuidExpressionGenerator<Product>, GuidExpressionGenerator<Product>>();
services.AddSingleton<IRestToLinqParser<Product>, RestToLinqParser<Product>>();
```

## Best Practices

### 1. Validate Query Parameters

```csharp
[HttpGet]
public IActionResult Get([FromQuery] string q)
{
    if (string.IsNullOrWhiteSpace(q))
    {
        q = "$sort_by=Id"; // Default query
    }
    
    try
    {
        var result = _parser.Run(_context.Products, q);
        return Ok(result.Data.ToList());
    }
    catch (Exception ex)
    {
        return BadRequest(ex.Message);
    }
}
```

### 2. Limit Maximum Page Size

```csharp
var result = _parser.Run(_context.Products, query);

// Enforce maximum page size
if (result.PageSize > 100)
{
    result.PageSize = 100;
}
```

### 3. Use Projection for Performance

```csharp
var result = _parser.Run(_context.Products, query);

var data = result.Data
    .Select(p => new ProductDto 
    { 
        Id = p.Id,
        Name = p.Name,
        Price = p.Price 
    })
    .ToList();
```

### 4. Add Default Filters

```csharp
var result = _parser.Parse(query);

IQueryable<Product> products = _context.Products
    .Where(p => p.IsActive); // Always filter active products

foreach (var expression in result.Expressions)
{
    products = products.Where(expression);
}
```

## Limitations

- **Case Sensitivity**: String comparisons are case-sensitive. `name=iphone` will not match `"iPhone"`.
- **Contains Operator**: Only available for string fields.
- **Complex Queries**: Does not support OR conditions, grouping, or nested queries.
- **Client-Side Evaluation**: Some operations may trigger client-side evaluation if used with certain LINQ providers.

## Performance Considerations

- ✅ Expressions are compiled once and reused
- ✅ Works directly with `IQueryable` for database query optimization
- ✅ Pagination is applied at the database level (when using EF Core)
- ⚠️ String `Contains` operations may not use indexes in some databases
- ⚠️ Sorting by multiple columns may impact performance on large datasets

## Version History

See [Release Notes](https://github.com/BigBadJock/REST-Parser/releases) for detailed version history.

### Recent Updates
- **1.2.5** - Target .NET 10
- **1.2.4** - Updated packages
- **1.2.3** - Fixed nullable boolean issue
- **1.2.2** - Added default sort order (Id) when none specified
- **1.2.0** - Upgraded to .NET 7

## Contributing

Contributions are welcome! Please feel free to submit issues or pull requests on [GitHub](https://github.com/BigBadJock/REST-Parser).

## License

Copyright © 2025 John McArthur

## Support

- **GitHub Issues**: [Report bugs or request features](https://github.com/BigBadJock/REST-Parser/issues)
- **NuGet**: [REST-Parser Package](https://www.nuget.org/packages/REST-Parser/)

## Related Projects

This library is ideal for:
- RESTful APIs with dynamic querying
- Admin dashboards with filtering and sorting
- Data grids with server-side processing
- Report generation with flexible parameters
- GraphQL-style field filtering in REST APIs

---

Made with ❤️ by [John McArthur](https://github.com/BigBadJock)
