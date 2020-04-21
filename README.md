# REST-Parser
A C# Parser for REST requests generating Linq expressions

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
