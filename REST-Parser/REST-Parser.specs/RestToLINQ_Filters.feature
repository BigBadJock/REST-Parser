Feature: RestToLINQ_Filters
	In order to generate a linq expression for an API request containing an equals filter
	As a parse
	I want to be able to parse the filter

@filter
Scenario: Add equals filter to surname 
	Given I have received a  request "surname[eq]=McArthur"
	When I parse it to linq
	Then the result should be a collection of Linq Expressions with an entry
