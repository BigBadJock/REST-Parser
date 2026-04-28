# Add support for OR conditions using comma-separated values (IN queries)

**Labels:** `enhancement`, `feature`

---

## Summary

Currently, REST-Parser only supports AND conditions (all filters must match). Add support for OR conditions using comma-separated values to enable SQL-like IN queries.

## Motivation

Users frequently need to filter by multiple values for the same field. For example:
- Get products in multiple categories: `category=Electronics,Computers,Gaming`
- Filter by multiple IDs: `id=1,2,3,4,5`
- Search multiple status values: `status=Pending,InProgress,Review`

This is a common REST API pattern and would significantly improve the library's flexibility without adding complexity to the query syntax.

## Current Behavior

Currently, only one value per field is supported:
```
GET /api/products?category=Electronics  // ✅ Works
GET /api/products?category=Electronics,Computers  // ❌ Fails or unexpected behavior
```

Users must make multiple requests or handle OR logic server-side, which defeats the purpose of dynamic querying.

## Proposed Solution

Implement comma-separated value support for the equality (`eq`) operator, which translates to an SQL IN clause or OR expression.

### Query Syntax

```
field=value1,value2,value3
field[eq]=value1,value2,value3  // Explicit syntax
```

### Generated Expression

```csharp
// Translates to:
p => p.Field == value1 || p.Field == value2 || p.Field == value3

// Or in SQL:
WHERE Field IN (value1, value2, value3)
```

### Examples

```
# Multiple categories
GET /api/products?category=Electronics,Computers,Gaming

# Multiple IDs
GET /api/products?id=10,20,30,40,50

# Combined with AND conditions
GET /api/products?category=Electronics,Gaming&price[lt]=1000&isActive=true

# With sorting and pagination
GET /api/products?category=Electronics,Computers&$sort_by=price[ASC]&$page=1&$pagesize=20
```

## Implementation Details

### 1. Modify `ParseCondition` Method

Add detection for comma-separated values in `RestToLinqParser.cs`:

```csharp
private Expression<Func<T, bool>> ParseCondition(string condition)
{
    // ... existing parsing logic ...
    
    // Check for comma-separated values (IN clause)
    if (restOperator == "eq" && value.Contains(','))
    {
        return BuildInExpression(parameter, field, value, paramType);
    }
    
    // ... existing switch statement ...
}
```

### 2. Add `BuildInExpression` Method

```csharp
private Expression<Func<T, bool>> BuildInExpression(
    ParameterExpression parameter, 
    string field, 
    string commaSeparatedValues, 
    Type fieldType)
{
    var values = commaSeparatedValues
        .Split(',', StringSplitOptions.RemoveEmptyEntries)
        .Select(v => v.Trim())
        .ToArray();

    if (values.Length == 0)
    {
        throw new REST_InvalidValueException(field, commaSeparatedValues);
    }

    var property = Expression.Property(parameter, field);
    Expression orExpression = null;

    foreach (var value in values)
    {
        try
        {
            object convertedValue = ConvertValue(value, fieldType);
            var constant = Expression.Constant(convertedValue, property.Type);
            var equality = Expression.Equal(property, constant);

            orExpression = orExpression == null 
                ? equality 
                : Expression.OrElse(orExpression, equality);
        }
        catch (Exception ex)
        {
            throw new REST_InvalidValueException(field, value, ex);
        }
    }

    return Expression.Lambda<Func<T, bool>>(orExpression, parameter);
}
```

### 3. Add `ConvertValue` Helper Method

```csharp
private object ConvertValue(string value, Type targetType)
{
    return Type.GetTypeCode(targetType) switch
    {
        TypeCode.String => value,
        TypeCode.Int32 => int.Parse(value),
        TypeCode.Double => double.Parse(value),
        TypeCode.Decimal => decimal.Parse(value),
        TypeCode.DateTime => DateTime.Parse(value),
        TypeCode.Boolean => bool.Parse(value),
        TypeCode.Object when targetType == typeof(Guid) => Guid.Parse(value),
        _ => throw new NotSupportedException($"Type {targetType} not supported for IN queries")
    };
}
```

## Constraints

1. **Operator Limitation**: Only works with `eq` (equality) operator
   - ✅ `category=Electronics,Gaming`
   - ✅ `category[eq]=Electronics,Gaming`
   - ❌ `price[gt]=100,200,300` (not supported - throw exception)

2. **Comma Escaping**: Values containing commas are not supported in this implementation
   - Could be enhanced later with URL encoding if needed

3. **Type Support**: Should work with all supported types:
   - string, int, double, decimal, DateTime, bool, Guid
   - Including nullable variants

## Acceptance Criteria

- [ ] Comma-separated values work for all supported data types
- [ ] Works with nullable types (`int?`, `DateTime?`, etc.)
- [ ] Throws `REST_InvalidOperatorException` if used with operators other than `eq`
- [ ] Throws `REST_InvalidValueException` if any value cannot be converted
- [ ] Can be combined with other AND conditions
- [ ] Can be combined with sorting and pagination
- [ ] All existing tests continue to pass
- [ ] New test cases added:
  - [ ] Multiple string values
  - [ ] Multiple integer values
  - [ ] Multiple GUID values
  - [ ] Multiple DateTime values
  - [ ] Mixed with other filters
  - [ ] Invalid values in list
  - [ ] Invalid operator with comma-separated values
  - [ ] Empty values
  - [ ] Single value with comma (should still work)
- [ ] Documentation updated in README.md and USAGE.md
- [ ] Copilot instructions updated

## Test Cases

```csharp
[TestMethod]
public void Test_IN_Clause_Multiple_Categories()
{
    // Arrange
    string query = "category=Electronics,Gaming,Computers";
    
    // Act
    var result = parser.Run(data, query);
    
    // Assert
    Assert.IsTrue(result.Data.All(p => 
        p.Category == "Electronics" || 
        p.Category == "Gaming" || 
        p.Category == "Computers"));
}

[TestMethod]
public void Test_IN_Clause_Multiple_IDs()
{
    // Arrange  
    string query = "id=1,3,5";
    
    // Act
    var result = parser.Run(data, query);
    
    // Assert
    Assert.AreEqual(3, result.Data.Count());
    Assert.IsTrue(result.Data.All(p => p.Id == 1 || p.Id == 3 || p.Id == 5));
}

[TestMethod]
public void Test_IN_Clause_With_Other_Filters()
{
    // Arrange
    string query = "category=Electronics,Gaming&price[lt]=1000&isActive=true";
    
    // Act
    var result = parser.Run(data, query);
    
    // Assert
    Assert.IsTrue(result.Data.All(p => 
        (p.Category == "Electronics" || p.Category == "Gaming") &&
        p.Price < 1000 &&
        p.IsActive));
}

[TestMethod]
[ExpectedException(typeof(REST_InvalidOperatorException))]
public void Test_IN_Clause_Invalid_Operator()
{
    // Arrange
    string query = "price[gt]=100,200,300";
    
    // Act & Assert
    parser.Run(data, query);
}

[TestMethod]
[ExpectedException(typeof(REST_InvalidValueException))]
public void Test_IN_Clause_Invalid_Value()
{
    // Arrange
    string query = "id=1,abc,3";
    
    // Act & Assert
    parser.Run(data, query);
}

[TestMethod]
public void Test_IN_Clause_Nullable_Types()
{
    // Arrange
    string query = "orderCount=1,3,5";
    
    // Act
    var result = parser.Run(data, query);
    
    // Assert
    Assert.IsTrue(result.Data.All(p => 
        p.OrderCount == 1 || p.OrderCount == 3 || p.OrderCount == 5));
}

[TestMethod]
public void Test_IN_Clause_DateTime()
{
    // Arrange
    string query = "birthday=1966-01-01,1968-01-01,1970-01-01";
    
    // Act
    var result = parser.Run(data, query);
    
    // Assert
    Assert.AreEqual(3, result.Data.Count());
}

[TestMethod]
public void Test_IN_Clause_Guid()
{
    // Arrange
    string guid1 = Guid.NewGuid().ToString();
    string guid2 = Guid.NewGuid().ToString();
    string query = $"guidId={guid1},{guid2}";
    
    // Act
    var result = parser.Run(data, query);
    
    // Assert
    Assert.IsTrue(result.Data.All(p => 
        p.GuidId.ToString() == guid1 || p.GuidId.ToString() == guid2));
}

[TestMethod]
public void Test_IN_Clause_Boolean()
{
    // Arrange
    string query = "flag=true,false";
    
    // Act
    var result = parser.Run(data, query);
    
    // Assert - Should return all records since it's true OR false
    Assert.AreEqual(data.Count(), result.Data.Count());
}

[TestMethod]
public void Test_IN_Clause_Single_Value_Still_Works()
{
    // Arrange
    string query = "category=Electronics";
    
    // Act
    var result = parser.Run(data, query);
    
    // Assert
    Assert.IsTrue(result.Data.All(p => p.Category == "Electronics"));
}
```

## Documentation Updates

Add to **USAGE.md**:

````markdown
## OR Conditions (IN Queries)

You can specify multiple values for a field using comma-separated values. This creates an OR condition (similar to SQL IN clause).

### Syntax
```
field=value1,value2,value3
field[eq]=value1,value2,value3
```

### Examples

```
# Products in Electronics OR Computers category
GET /api/products?category=Electronics,Computers

# Products with specific IDs
GET /api/products?id=10,20,30,40,50

# Combine with other filters (AND logic)
GET /api/products?category=Electronics,Gaming&price[lt]=1000&isActive=true

# With sorting and pagination
GET /api/products?category=Electronics,Computers,Gaming&$sort_by=price[ASC]&$page=1&$pagesize=20

# Works with all supported types
GET /api/products?rating=4.0,4.5,5.0
GET /api/products?releaseDate=2023-01-01,2024-01-01
GET /api/products?isActive=true,false
```

### Supported Types

Comma-separated values work with all supported data types:
- ✅ `string`
- ✅ `int` / `int?`
- ✅ `double` / `double?`
- ✅ `decimal` / `decimal?`
- ✅ `DateTime` / `DateTime?`
- ✅ `bool` / `bool?`
- ✅ `Guid` / `Guid?`

### Limitations

- **Only works with equality** (`eq`) operator (explicit or default)
- Cannot be used with comparison operators (`gt`, `lt`, `ge`, `le`, `ne`, `contains`)
- Values containing commas are not supported (would need URL encoding)

### Error Handling

```csharp
// ❌ Invalid: Using with non-equality operator
GET /api/products?price[gt]=100,200,300
// Throws: REST_InvalidOperatorException

// ❌ Invalid: Value type mismatch
GET /api/products?id=1,abc,3
// Throws: REST_InvalidValueException
```
````

Add to **README.md** (Features section):

```markdown
✨ **Dynamic Filtering** - Support for equality, comparison, contains, and **OR conditions**
```

Update **README.md** (Query Syntax section):

````markdown
### OR Conditions

Use comma-separated values for OR logic on the same field:

```
category=Electronics,Computers,Gaming
```

Translates to: `category == "Electronics" OR category == "Computers" OR category == "Gaming"`
````

## Alternative Approaches Considered

### 1. Pipe Separator (`field=value1|value2`)
- **Pro**: Distinct from commas
- **Con**: Less common in REST APIs

### 2. Special OR Parameter (`$or[0][field]=value1&$or[1][field]=value2`)
- **Pro**: Most flexible, supports complex OR groups
- **Con**: Complex query strings, harder to use

### 3. Repeated Parameters (`field=value1&field=value2`)
- **Pro**: Common in some APIs (e.g., query parameters in some frameworks)
- **Con**: Requires more significant parsing changes, conflicts with AND logic

**Comma-separated was chosen** because:
- Most common REST pattern (similar to OData, many public APIs)
- Simplest to implement
- URL-friendly
- Intuitive for users

## Breaking Changes

**None.** This is fully backward compatible:
- Existing queries continue to work unchanged
- New syntax is opt-in (only activated when comma is detected in value)
- No changes to existing API surface

## Version

Proposed for version **1.3.0** (minor version bump for new feature)

Update `PackageReleaseNotes` to:
```
1.3.0 - Add OR condition support via comma-separated values, upgrade to .NET 10, code improvements
```

---

## Additional Context

### Why This Feature is Important

1. **Industry Standard**: Similar libraries (OData, GraphQL filters, Elastic Search) support IN-style queries
2. **User Expectations**: Developers expect standard REST patterns
3. **Performance**: Single request instead of multiple requests or complex server-side logic
4. **Flexibility**: Covers 90% of OR use cases without complex syntax

### Related Examples from Other APIs

**OData:**
```
$filter=Category in ('Electronics','Computers')
```

**GraphQL:**
```
{ products(category_in: ["Electronics", "Computers"]) }
```

**Elasticsearch:**
```json
{ "terms": { "category": ["Electronics", "Computers"] } }
```

REST-Parser equivalent:
```
category=Electronics,Computers
```

### Future Enhancements

If this feature is successful, future versions could add:
1. Support for `ne` operator with comma-separated values (NOT IN)
2. URL-encoded comma support for values containing commas
3. Special syntax for complex OR groups: `(A AND B) OR (C AND D)`

---

## Checklist for Implementation

- [ ] Create feature branch: `feature/or-conditions-comma-separated`
- [ ] Implement `BuildInExpression` method
- [ ] Implement `ConvertValue` helper method
- [ ] Update `ParseCondition` to detect comma-separated values
- [ ] Add comprehensive test cases (minimum 10 tests)
- [ ] Verify all existing 116 tests still pass
- [ ] Update USAGE.md documentation
- [ ] Update README.md documentation
- [ ] Update `.github/copilot-instructions.md`
- [ ] Update version to 1.3.0 in `.csproj`
- [ ] Update release notes in `.csproj`
- [ ] Build and verify no compilation errors
- [ ] Run all tests and verify 100% pass rate
- [ ] Create pull request
- [ ] Code review
- [ ] Merge to main
- [ ] Create GitHub release v1.3.0
- [ ] Publish to NuGet

---

**Priority:** Medium  
**Estimated Effort:** 4-6 hours  
**Dependencies:** None  
**Assignee:** @BigBadJock

---

## Questions?

Please comment on this issue if you have:
- Alternative syntax suggestions
- Additional test cases to consider
- Performance concerns
- Edge cases we should handle
