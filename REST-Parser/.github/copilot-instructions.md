# REST-Parser Copilot Instructions

## Project Overview
REST-Parser is a .NET library that converts REST query strings into LINQ expressions and executes them against IQueryable data sources. It supports filtering, sorting, and pagination through a simple REST-style query syntax.

## Architecture

### Core Components
- **RestToLinqParser<T>**: Main parser class that converts REST queries to LINQ expressions
- **IRestToLinqParser<T>**: Public interface for the parser
- **Expression Generators**: Type-specific generators for creating LINQ expressions (String, Int, Date, Double, Decimal, Boolean, Guid)
- **RestResult<T>**: Result object containing parsed expressions, sort orders, and pagination metadata

### Supported Operations
- **Filtering**: `eq` (equal), `ne` (not equal), `gt` (greater than), `ge` (greater than or equal), `lt` (less than), `le` (less than or equal), `contains`
- **Sorting**: `$sort_by=FieldName[ASC|DESC]`
- **Pagination**: `$page=n`, `$pagesize=n`

## Code Style & Conventions

### Naming
- Use underscore-prefixed names for exception classes: `REST_InvalidFieldnameException`
- Use descriptive camelCase for private fields: `stringExpressionGenerator`
- Use PascalCase for public properties and methods

### Expression Generation
- Each type has its own expression generator implementing `IExpressionGenerator<T>`
- Generators handle nullable types by checking for `Nullable<>` generic type
- Always validate field names and throw `REST_InvalidFieldnameException` for invalid fields
- Always validate operators and throw `REST_InvalidOperatorException` for invalid operators
- Always validate values and throw `REST_InvalidValueException` for invalid values

### REST Query Format
- Basic filter: `fieldname=value` (defaults to `eq` operator)
- With operator: `fieldname[operator]=value`
- Whitespace is trimmed: `fieldname [operator] = value` is valid
- Multiple conditions: separated by `&`
- Case-insensitive special keywords: `$SORT_BY`, `$PAGE`, `$PAGESIZE`

### Default Behavior
- If no sort order is specified, defaults to `$sort_by=Id[ASC]`
- If page is specified without pagesize, pagesize defaults to 25
- If pagesize is specified without page, page defaults to 1

### Testing
- Use MSTest framework (`[TestClass]`, `[TestMethod]`, `[DataTestMethod]`, `[DataRow]`)
- Test initialization in `[TestInitialize]` method
- Test class naming: `RestToLinq_{Feature}_Tests` (e.g., `RestToLinq_Equal_Tests`)
- Test method naming: Descriptive with operation and type (e.g., `EQ_String_Test`, `SORT_String_Ascending`)
- Use `TestItem` class for test data with various property types
- Always test nullable and non-nullable scenarios

### Error Handling
- Catch specific exceptions and rethrow custom REST exceptions
- Don't swallow exceptions - always provide context
- Custom exceptions: `REST_InvalidFieldnameException`, `REST_InvalidOperatorException`, `REST_InvalidValueException`

### Dependency Injection
- Use `StartupExtensions.RegisterRestParser<T>()` extension method
- All expression generators are registered as singletons
- Parser itself is registered as singleton

## When Adding New Features

1. **New Type Support**:
   - Create `I{Type}ExpressionGenerator<T>` interface
   - Create `{Type}ExpressionGenerator<T>` implementation
   - Add to `RestToLinqParser<T>` constructor and switch statement
   - Register in `StartupExtensions.RegisterRestParser<T>()`
   - Add comprehensive tests

2. **New Operators**:
   - Add case to relevant expression generator's switch statement
   - Throw `REST_InvalidOperatorException` if not supported
   - Update tests for all affected types
   - Document in release notes

3. **New Query Features**:
   - Add parsing logic in `Parse()` method
   - Update `RestResult<T>` if new metadata is needed
   - Add execution logic in `Run()` method
   - Create dedicated test class

## Important Implementation Details

### Nullable Handling
- Check for `Nullable<>` generic type definition
- Extract underlying type using `Nullable.GetUnderlyingType()`
- String `Contains` operation includes null check: `p.Field != null && p.Field.Contains(value)`

### Expression Building
- Use `Expression.Parameter(typeof(T), "p")` for consistent parameter naming
- Use `Expression.PropertyOrField()` to access properties
- Use `Expression.Convert()` for type conversions
- Use `Expression.Lambda<Func<T, bool>>()` to create predicate expressions

### Sorting
- First sort uses `OrderBy` or `OrderByDescending`
- Subsequent sorts use `ThenBy` or `ThenByDescending`
- Sort expressions are `Expression<Func<T, object>>` with conversion to object

### Pagination
- Calculate `TotalCount` before pagination
- Calculate `PageCount` based on total and page size
- Use `Skip()` and `Take()` for pagination
- Ensure page number doesn't exceed page count

## Don't Change
- The REST query syntax format (breaking change)
- Exception naming convention with REST_ prefix
- Public API surface without version bump
- Default behaviors (Id sorting, page size 25)
- Expression generator pattern

## Target Framework
- Currently targeting .NET 10.0
- Use modern C# features where appropriate
- Maintain compatibility with IQueryable data sources

## Package Information
- NuGet package: `REST-Parser`
- Author: John McArthur
- GitHub: https://github.com/BigBadJock/REST-Parser
- Update version in .csproj and release notes when making changes
