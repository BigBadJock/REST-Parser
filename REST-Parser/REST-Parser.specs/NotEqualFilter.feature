Feature: NotEqualFilter
	In order to generate a sql query for an API request
	As a parse
	I want to be able to parse the filter including a [ne] not equals filter

@filter
Scenario Outline: Add not equals filter to surname 
	Given I have received a <request> request 
	When I parse it
	Then the result should be a sql query <expectedSQL>
	Examples:
	| request              | expectedSQL           |
	| surname[ne]=McArthur | surname <> 'McArthur' |
	| surname[ne]=Smith    | surname <> 'Smith'    |
