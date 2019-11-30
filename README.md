# REST-Parser
A C# Parser for REST requests generating Linq expressions

## Progress
### 30/11/2019
Changed result from parse to RestResult object, which contains expressions, sort expressions with paging to come.
Added Run command to parser which takes a IQueryable data source, and the rest request, parses, runs against the data  and returns the results. 

### 25/11/2019
Refactored. Removed SQL generator. 

### 22/11/2019
Removed Specflow tests.
Added unit tests.
tests added for RestToLinq String Equals
