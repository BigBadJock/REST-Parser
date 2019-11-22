using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using TechTalk.SpecFlow;

namespace REST_Parser.specs
{
    [Binding]
    public class RestToLINQ_FiltersSteps
    {

        RestToLinqParser<TestItem> parser = new RestToLinqParser<TestItem>();
        string request;
        List<Expression<Func<TestItem, bool>>> result;



        [Given(@"I have received a  request ""(.*)""")]
        public void GivenIHaveReceivedARequest(string p0)
        {
            request = p0;
        }

        [When(@"I parse it to linq")]
        public void WhenIParseIt()
        {
            this.result = (List<Expression<Func<TestItem, bool>>>)parser.Parse(this.request);
        }

        [Then(@"the result should be a collection of Linq Expressions with an entry")]
        public void ThenTheResultShouldBeACollectionOfLinqExpressionsWithAnEntry()
        {
            Assert.IsInstanceOfType(result, typeof(List<Expression<Func<TestItem, bool>>>));
            Assert.IsTrue(result.Count == 1);

            var condition = result[0];
            System.Diagnostics.Debug.WriteLine(condition.ToString());
        }
    }
}
