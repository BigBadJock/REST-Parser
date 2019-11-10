Feature: Filter
	In order to generate a sql query for an API request containing an equals filter
	As a parse
	I want to be able to parse the filter

@filter
Scenario Outline: Add equals filter to surname 
	Given I have received a <request> request 
	When I parse it
	Then the result should be a sql query <expectedSQL>
	Examples:
	| request              | expectedSQL        |
	| surname[eq]=McArthur | surname = 'McArthur' |
	| surname[eq]=Smith    | surname = 'Smith'    |


@filter
Scenario Outline: Add equals filter to firstname 
	Given I have received a <request> request
	When I parse it
	Then the result should be a sql query <expectedSQL>
	Examples:
	| request              | expectedSQL        |
	| firstname[eq]=John   | firstname = 'John'   |
	| firstname[eq]=James  | firstname = 'James'  |

@filter
Scenario Outline: Add equals filter to price 
	Given I have received a <request> request
	When I parse it
	Then the result should be a sql query <expectedSQL>
	Examples:
	| request			| expectedSQL       |
	| price[eq]=55		| price = 55		|
	| price[eq]=55.25	| price = 55.25 |


