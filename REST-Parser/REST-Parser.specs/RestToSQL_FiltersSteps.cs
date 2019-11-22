using System;
using TechTalk.SpecFlow;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using TechTalk.SpecFlow.Assist;

namespace REST_Parser.specs
{
    [Binding]
    public class RestToSQL_FiltersSteps
    {
        RestToSQLParser parser = new RestToSQLParser();
        string request;
        string result;
 
        [Given(@"I have received a (.*) request")]
        public void GivenIHaveReceivedARequestWithAnEqualsEqFilters(string request)
        {
            this.request = request;
        }


        [When(@"I parse it")]
        public void WhenIParseIt()
        {
            this.result = (string)parser.Parse(this.request);
        }

        [Then(@"the result should be a sql query (.*)")]
        public void ThenTheResultShouldBeASqlQuerySegmentWiths(string expectedSQL)
        {
            Assert.AreEqual(expectedSQL, this.result);
        }



    }
}
