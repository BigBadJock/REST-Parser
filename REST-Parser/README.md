# REST-Parser

[![NuGet](https://img.shields.io/nuget/v/REST-Parser.svg)](https://www.nuget.org/packages/REST-Parser/)
[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)

REST-Parser converts REST-style query strings into LINQ expressions and can run them against an `IQueryable<T>`. It is designed for APIs that need dynamic filtering, sorting, and pagination without hand-writing a query endpoint for every entity.

## Features

- Dynamic filtering with equality, comparison, and string `contains` operators
- Multi-column sorting with ascending and descending directions
- Optional page-based pagination with result metadata
- Strongly typed LINQ expression generation
- Dependency injection support for ASP.NET Core
- Nullable support for supported scalar types
- Guardrails for query length, condition count, and page size

## Contents

- [Installation](#installation)
- [Quick Start](#quick-start)
- [Query Syntax](#query-syntax)
- [Filtering](#filtering)
- [Sorting](#sorting)
- [Pagination](#pagination)
- [API Usage](#api-usage)
- [Exception Handling](#exception-handling)
- [Limits and Behavior](#limits-and-behavior)
- [Troubleshooting](#troubleshooting)
- [Version History](#version-history)

## Installation

Install the NuGet package:

```bash
dotnet add package REST-Parser
```

Package Manager:

```powershell
Install-Package REST-Parser
```

Package reference:

```xml
<PackageReference Include="REST-Parser" Version="1.4.0" />
```

## Quick Start

Define an entity:

```csharp
public class Product
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public string Name { get; set; } = "";
    public string Category { get; set; } = "";
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public DateTime ReleaseDate { get; set; }
    public bool IsActive { get; set; }
    public string? Description { get; set; }
    public double? Rating { get; set; }
}
```

Register the parser for each entity type you want to query:

```csharp
using REST_Parser.DependencyResolution;

builder.Services.RegisterRestParser<Product>();
```

Inject and run the parser:

```csharp
using REST_Parser;
using REST_Parser.Models;

public class ProductService
{
    private readonly IRestToLinqParser<Product> parser;
    private readonly AppDbContext context;

    public ProductService(IRestToLinqParser<Product> parser, AppDbContext context)
    {
        this.parser = parser;
        this.context = context;
    }

    public RestResult<Product> GetProducts(string query)
    {
        return parser.Run(context.Products, query);
    }
}
```

Example query:

```http
GET /api/products?category=Electronics&price[lt]=1000&$sort_by[asc]=price&$page=1&$pagesize=20
```

## Query Syntax

The general filter format is:

```text
field[operator]=value
```

If you omit the operator, `eq` is used:

```text
name=iPhone
name[eq]=iPhone
```

Combine query parts with `&`:

```text
category=Electronics&price[lt]=1000&isActive=true
```

Special parameters:

| Parameter | Purpose | Example |
| --- | --- | --- |
| `$sort_by` | Sort by a field | `$sort_by[desc]=price` |
| `$page` | One-based page number | `$page=2` |
| `$pagesize` | Items per page | `$pagesize=25` |

Parameter names for `$sort_by`, `$page`, and `$pagesize` are matched case-insensitively. Some HTTP clients encode brackets and dollar signs automatically; for example, `$sort_by%5Bdesc%5D=price` is equivalent once decoded by ASP.NET Core.

## Filtering

Supported operators:

| Operator | Description | Supported types |
| --- | --- | --- |
| `eq` | Equal to | All supported types |
| `ne` | Not equal to | All supported types |
| `gt` | Greater than | `int`, `double`, `decimal`, `DateTime` |
| `ge` | Greater than or equal to | `int`, `double`, `decimal`, `DateTime` |
| `lt` | Less than | `int`, `double`, `decimal`, `DateTime` |
| `le` | Less than or equal to | `int`, `double`, `decimal`, `DateTime` |
| `contains` | String contains | `string` |

Supported CLR types:

- `string`
- `int` and `int?`
- `double` and `double?`
- `decimal` and `decimal?`
- `DateTime` and `DateTime?`
- `bool` and `bool?`
- `Guid` and `Guid?`

String examples:

```text
name=iPhone
name[ne]=Samsung
description[contains]=wireless
category=Electronics&brand=Apple
```

Numeric examples:

```text
stock[gt]=10
stock[le]=100
price[lt]=999.99
rating[ge]=4.5
```

Date examples:

```text
releaseDate[gt]=2023-01-01
releaseDate[ge]=2023-01-01&releaseDate[le]=2023-12-31
```

Boolean and GUID examples:

```text
isActive=true
isDiscontinued[ne]=true
productId[eq]=123e4567-e89b-12d3-a456-426614174000
```

## Sorting

Sort with `$sort_by`. Put the optional direction on the `$sort_by` parameter and the field name on the right side:

```text
$sort_by=name
$sort_by[asc]=name
$sort_by[desc]=price
```

Multiple sort clauses are applied in order:

```text
category=Electronics&$sort_by[asc]=brand&$sort_by[desc]=price
```

If no sort is supplied and the entity has an `Id` property, REST-Parser sorts by `Id` ascending. If the entity has no `Id` property, no default sort is added.

## Pagination

Pagination is optional. Add `$page` and/or `$pagesize` to enable it:

```text
$page=2&$pagesize=25
```

Behavior:

- `$page` is one-based.
- If `$page` is supplied without `$pagesize`, page size defaults to `25`.
- If `$pagesize` is supplied without `$page`, page defaults to `1`.
- If neither parameter is supplied, results are not paginated.
- Page size is capped at `1000`.
- If the requested page is past the end, the parser moves it to the last available page.

`RestResult<T>` includes:

| Property | Description |
| --- | --- |
| `Data` | Filtered, sorted, and optionally paginated `IQueryable<T>` from `Run()` |
| `Page` | Current page number |
| `PageSize` | Items per page |
| `TotalCount` | Total matching items before pagination |
| `PageCount` | Total number of pages |
| `Expressions` | Filter expressions parsed from the query |
| `SortOrder` | Sort expressions parsed from the query |

## API Usage

### ASP.NET Core Controller

To support direct query strings such as `/api/products?category=Electronics&price[lt]=1000`, pass the raw request query string to the parser:

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using REST_Parser;
using REST_Parser.Models;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IRestToLinqParser<Product> parser;
    private readonly AppDbContext context;

    public ProductsController(IRestToLinqParser<Product> parser, AppDbContext context)
    {
        this.parser = parser;
        this.context = context;
    }

    [HttpGet]
    public IActionResult Get()
    {
        string query = Request.QueryString.Value?.TrimStart('?') ?? "";
        RestResult<Product> result = parser.Run(context.Products.AsNoTracking(), query);

        return Ok(new
        {
            data = result.Data.Select(p => new
            {
                p.Id,
                p.Name,
                p.Category,
                p.Price
            }).ToList(),
            pagination = new
            {
                page = result.Page,
                pageSize = result.PageSize,
                totalCount = result.TotalCount,
                pageCount = result.PageCount
            }
        });
    }
}
```

Sample requests:

```http
GET /api/products
GET /api/products?category=Electronics
GET /api/products?price[gt]=100&price[lt]=1000
GET /api/products?description[contains]=wireless&isActive=true
GET /api/products?category=Electronics&$sort_by[asc]=price&$page=1&$pagesize=10
```

### Parse vs Run

Use `Parse()` when you want to inspect or apply the generated pieces yourself:

```csharp
RestResult<Product> parsed = parser.Parse("category=Electronics&price[lt]=1000");

IQueryable<Product> query = context.Products
    .Where(p => p.IsActive)
    .Where(p => p.TenantId == tenantId);

foreach (var expression in parsed.Expressions)
{
    query = query.Where(expression);
}
```

Use `Run()` when you want REST-Parser to apply filters, sorting, and pagination in one call:

```csharp
RestResult<Product> result = parser.Run(
    context.Products.AsNoTracking(),
    "category=Electronics&price[lt]=1000&$sort_by[asc]=price&$page=1&$pagesize=20");

List<Product> products = result.Data.ToList();
```

### Manual Registration

The `RegisterRestParser<T>()` extension is the preferred setup. If needed, you can register the services manually:

```csharp
services.AddSingleton<IStringExpressionGenerator<Product>, StringExpressionGenerator<Product>>();
services.AddSingleton<IIntExpressionGenerator<Product>, IntExpressionGenerator<Product>>();
services.AddSingleton<IDateExpressionGenerator<Product>, DateExpressionGenerator<Product>>();
services.AddSingleton<IDoubleExpressionGenerator<Product>, DoubleExpressionGenerator<Product>>();
services.AddSingleton<IDecimalExpressionGenerator<Product>, DecimalExpressionGenerator<Product>>();
services.AddSingleton<IBooleanExpressionGenerator<Product>, BooleanExpressionGenerator<Product>>();
services.AddSingleton<IGuidExpressionGenerator<Product>, GuidExpressionGenerator<Product>>();
services.AddSingleton<IRestToLinqParser<Product>, RestToLinqParser<Product>>();
```

## Exception Handling

REST-Parser throws specific exceptions for invalid query parts:

```csharp
using REST_Parser.Exceptions;

try
{
    RestResult<Product> result = parser.Run(context.Products, query);
    return Ok(result.Data.ToList());
}
catch (REST_InvalidFieldnameException ex)
{
    return BadRequest(new { error = "Invalid field", details = ex.Message });
}
catch (REST_InvalidOperatorException ex)
{
    return BadRequest(new { error = "Invalid operator", details = ex.Message });
}
catch (REST_InvalidValueException ex)
{
    return BadRequest(new { error = "Invalid value", details = ex.Message });
}
catch (ArgumentException ex)
{
    return BadRequest(new { error = "Invalid query", details = ex.Message });
}
```

Common causes:

| Exception | Typical cause | Example |
| --- | --- | --- |
| `REST_InvalidFieldnameException` | Field does not exist on the entity | `unknownField=value` |
| `REST_InvalidOperatorException` | Operator is not supported for that field type | `name[gt]=test` |
| `REST_InvalidValueException` | Value cannot be converted to the field type | `price=abc` |
| `ArgumentException` | Query format or security limit failure | Too many conditions |

## Limits and Behavior

Built-in limits:

| Limit | Value |
| --- | --- |
| Maximum query length | `2000` characters |
| Maximum query parts | `50` parts separated by `&` |
| Maximum page size | `1000` |

Other behavior to be aware of:

- String equality and `contains` use the underlying .NET string comparison behavior and are case-sensitive for in-memory queries.
- `contains` is only available for `string` fields.
- Complex query grouping, nested expressions, and `OR` conditions are not supported.
- Numeric and date parsing uses invariant culture.
- `Run()` keeps the result as `IQueryable<T>`, so database-backed providers such as EF Core can translate the generated query where supported.

## Best Practices

- Use `AsNoTracking()` for read-only EF Core query endpoints.
- Apply tenant, user, or soft-delete filters before user-provided filters when you need isolation.
- Project to DTOs before returning API responses.
- Catch parser exceptions and return clear `400 Bad Request` responses.
- Keep API-specific page size limits stricter than `1000` if your endpoint needs a smaller cap.

Example with tenant isolation:

```csharp
RestResult<Product> parsed = parser.Parse(query);

IQueryable<Product> products = context.Products
    .Where(p => p.TenantId == tenantId)
    .Where(p => p.IsActive);

foreach (var expression in parsed.Expressions)
{
    products = products.Where(expression);
}
```

## Troubleshooting

### Invalid field name

Check that the field name maps to a public property on `T`:

```text
name=iPhone
price[lt]=1000
```

### Invalid operator

Use an operator supported by the field type. For example, `contains` is valid for strings, but not for numbers or dates.

### Invalid value

Make sure the value can be parsed as the field type:

```text
price=999.99
releaseDate=2023-01-01
isActive=true
```

### No pagination metadata

`Page`, `PageSize`, `TotalCount`, and `PageCount` are populated by `Run()` when pagination is requested. Add `$page` or `$pagesize` if the endpoint needs those values.

### No results

Inspect the parsed expressions and test the base query before filters:

```csharp
RestResult<Product> parsed = parser.Parse(query);

Console.WriteLine($"Filters: {parsed.Expressions.Count}");
Console.WriteLine($"Sorts: {parsed.SortOrder.Count}");
Console.WriteLine($"Base count: {context.Products.Count()}");
```

## Version History

Recent updates:

- `1.4.0` - Bug fixes for value splitting on `=`, nullable handling, culture-invariant parsing, exception accuracy, null-safe collection initialization, and unsupported type detection.
- `1.3.0` - Upgrade to .NET 10 with code improvements and optimizations.
- `1.2.4` - Package updates.
- `1.2.3` - Nullable boolean fix.
- `1.2.2` - Default `Id` sort when no sort is supplied.
- `1.2.0` - Upgrade to .NET 7.

See [GitHub releases](https://github.com/BigBadJock/REST-Parser/releases) for more detail.

## Links

- [NuGet package](https://www.nuget.org/packages/REST-Parser/)
- [GitHub repository](https://github.com/BigBadJock/REST-Parser)
- [Report issues](https://github.com/BigBadJock/REST-Parser/issues)

## License

Copyright (c) 2026 John McArthur.
