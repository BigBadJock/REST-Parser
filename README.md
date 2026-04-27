# REST-Parser
A C# Parser for REST requests generating Linq expressions


The REST query allows for filtering, sorting and paging.

## Filters
Filters work with all types

| Operator | Description | Example | Field Types | Comparison Operators |
| ----- | ----- | ----- | ----- | ----- |
| eq | Equal | <ul><li>city[eq]=Redmond</li><li>id=[eq]=673171EA-813F-4CF8-B9D3-CBCBACBD4F89 </li><li>birthDate[eq]=2001-01-01</li><li>price[eq]=5.99</li><li>isActive[eq]=true</li></ul> | string, date, int, decimal, double, boolean | |
| ne | Not equal | <ul><li>city[ne]=London</li></ul> | string, date, int, decimal, double, boolean | |
| gt | Greater than | <ul><li>price[gt]=20</li><li>birthDate[gt]=2001-01-01</li></ul> | date, int, decimal, double | |
| ge | Greater than or equal | <ul><li>price[ge]=10</li><li>birthDate[ge]=2001-01-01</li></ul> | date, int, decimal, double | |
| lt | Less than | <ul><li>price[lt]=20</li><li>birthDate[lt]=2001-01-01</li></ul> | date, int, decimal, double | |
| le | Less than or equal | <ul><li>price[le]=100</li><li>birthDate[le]=2001-01-01</li></ul> | date, int, decimal, double | |
| contains | contains the value | <ul><li>surname[contains]=McA</li></ul> | string | |
| notcontains | does not contain the value | <ul><li>surname[notcontains]=Mc</li></ul> | string | |
| startswith | starts with the value | <ul><li>surname[startswith]=Mc</li></ul> | string | |
| isnull | checks if the value is null | <ul><li>surname[isnull]=false</li></ul> | string | |


> Note:
> The default filter is [eq]
> It is case insensitive

## Sorting
You can sort by multiple fields ascending or descending.

- GET /items?code=xxx&rest=$sort_by=surname
- GET /items?code=xxx&rest=$sort_by[asc]=surname
- GET /items?code=xxx&rest=$sort_by[desc]=surname
- GET /items?code=xxx&rest=$sort_by[asc]=surname&$sort_by[desc]=firstname

## Pagination
Pagination is optional, by default all records are returned. If pagination is required, a sort_by should also be added

- GET /items?code=xxx&rest=$page=1&$pageSize=10
- GET /items?code=xxx&rest=$page=2&$pageSize=10
- GET /items?code=xxx&rest=$page=5&$pageSize=25

## All Together
```
GET /items?code=xxx&rest=surname[contains]=Smi
    &hometown[eq]=Edinburgh
  &$sort_by[asc]=surname
  &$sort_by[desc]=firstname
  &$page=1
  &$pageSize=10
```

Results
The result is a JSON object with two properties Data, and Pagination:
``` JSON
{
    "data" : [
        {  "id":"1", "firstName":"Bob", "lastName": "Smith"..... },
        {  "id":"2", "firstName":"Rob", "lastName": "Smith"..... },
        ...
        {  }
    ],
    "Pagination": {
        "PageNumber": 1,
        "PageSize": 10,
        "PageCount": 2,
        "TotalCount": 19
    }
}
```

## Progress

### 21/04/2020
Fixed three issues including the \[contains\] on a nullable string.

### 30/12/2019
Updated to handle nullable types. There is a remaining issue throwing an exception when using \[contains\] on a nullable string.

### 30/11/2019
Changed result from parse to RestResult object, which contains expressions, sort expressions with paging to come.
Added Run command to parser which takes a IQueryable data source, and the rest request, parses, runs against the data  and returns the results. 

### 25/11/2019
Refactored. Removed SQL generator. 

### 22/11/2019
Removed Specflow tests.
Added unit tests.
tests added for RestToLinq String Equals
