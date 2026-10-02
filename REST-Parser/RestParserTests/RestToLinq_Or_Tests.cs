using Microsoft.VisualStudio.TestTools.UnitTesting;
using REST_Parser;
using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RestParserTests
{
    [TestClass]
    public class RestToLinq_Or_Tests
    {
        IQueryable<TestItem> data;
        RestToLinqParser<TestItem> parser;

        [TestInitialize]
        public void Initialize()
        {
            List<TestItem> d1 = new List<TestItem>();
            d1.Add(new TestItem { Id = 1, Surname = "Smith", Amount = 10, Price = 1.5, Status = TestStatus.Draft, OrderCount = 3 });
            d1.Add(new TestItem { Id = 2, Surname = "Jones", Amount = 20, Price = 2.5, Status = TestStatus.Active });
            d1.Add(new TestItem { Id = 3, Surname = "Brown", Amount = 30, Price = 3.5, Status = TestStatus.Archived, OrderCount = 5 });
            d1.Add(new TestItem { Id = 4, Surname = "Smith", Amount = 40, Price = 4.5, Status = TestStatus.Active, OrderCount = 7 });
            this.data = d1.AsQueryable();

            this.parser = new RestToLinqParser<TestItem>(
                new StringExpressionGenerator<TestItem>(),
                new IntExpressionGenerator<TestItem>(),
                new DateExpressionGenerator<TestItem>(),
                new DoubleExpressionGenerator<TestItem>(),
                new DecimalExpressionGenerator<TestItem>(),
                new BooleanExpressionGenerator<TestItem>(),
                new GuidExpressionGenerator<TestItem>());
        }

        private int[] Ids(string rest)
        {
            return parser.Run(data, rest).Data.Select(p => p.Id).ToArray();
        }

        [TestMethod]
        [DataRow("surname=Jones|surname=Brown", new[] { 2, 3 })]
        [DataRow("amount[lt]=15|amount[gt]=35", new[] { 1, 4 })]
        [DataRow("surname=Jones|amount[ge]=30", new[] { 2, 3, 4 })]
        [DataRow("status=Draft|status=Archived|price[eq]=2.5", new[] { 1, 2, 3 })]
        [DataRow("surname=Nobody|amount=999", new int[0])]
        [DataRow("orderCount[gt]=4|surname=Jones", new[] { 2, 3, 4 })]
        public void Or_MatchesAnyAlternative(string rest, int[] expectedIds)
        {
            CollectionAssert.AreEqual(expectedIds, Ids(rest));
        }

        [TestMethod]
        [DataRow("surname=Smith|surname=Jones&amount[gt]=15", new[] { 2, 4 })]
        [DataRow("amount[gt]=15&surname=Smith|surname=Jones", new[] { 2, 4 })]
        [DataRow("surname=Smith|surname=Brown&status=Active|status=Draft", new[] { 1, 4 })]
        public void Or_IsAndedWithOtherConditions(string rest, int[] expectedIds)
        {
            CollectionAssert.AreEqual(expectedIds, Ids(rest));
        }

        [TestMethod]
        public void Or_ProducesOneExpressionPerAndPart()
        {
            var result = parser.Parse("surname=Smith|surname=Jones&amount[gt]=15");
            Assert.AreEqual(2, result.Expressions.Count);
        }

        [TestMethod]
        public void Or_WorksWithSortingAndPaging()
        {
            var result = parser.Run(data, "surname=Smith|surname=Brown&$sort_by[desc]=amount&$pagesize=2");
            CollectionAssert.AreEqual(new[] { 4, 3 }, result.Data.Select(p => p.Id).ToArray());
        }

        [TestMethod]
        [DataRow("surname[in]=Jones,Brown", new[] { 2, 3 })]
        [DataRow("surname[in]=Jones, Brown", new[] { 2, 3 })]
        [DataRow("surname[in]=Smith", new[] { 1, 4 })]
        [DataRow("amount[in]=10,30,999", new[] { 1, 3 })]
        [DataRow("price[in]=2.5,4.5", new[] { 2, 4 })]
        [DataRow("status[in]=Draft,archived", new[] { 1, 3 })]
        [DataRow("orderCount[in]=3,7", new[] { 1, 4 })]
        [DataRow("surname[in]=Nobody,Else", new int[0])]
        public void In_MatchesAnyValue(string rest, int[] expectedIds)
        {
            CollectionAssert.AreEqual(expectedIds, Ids(rest));
        }

        [TestMethod]
        [DataRow("surname[in]=Smith,Jones&amount[gt]=15", new[] { 2, 4 })]
        [DataRow("surname[in]=Jones,Brown|amount=10", new[] { 1, 2, 3 })]
        public void In_CombinesWithAndAndOr(string rest, int[] expectedIds)
        {
            CollectionAssert.AreEqual(expectedIds, Ids(rest));
        }

        [TestMethod]
        public void In_InvalidValue()
        {
            Assert.ThrowsException<REST_InvalidValueException>(() => parser.Parse("amount[in]=10,abc"));
        }

        [TestMethod]
        public void Or_InvalidFieldInAlternative()
        {
            Assert.ThrowsException<REST_InvalidFieldnameException>(() => parser.Parse("surname=Smith|nosuchfield=1"));
        }

        [TestMethod]
        public void Or_InvalidOperatorInAlternative()
        {
            Assert.ThrowsException<REST_InvalidOperatorException>(() => parser.Parse("surname=Smith|amount[contains]=1"));
        }

        [TestMethod]
        [DataRow("surname=Smith|")]
        [DataRow("|surname=Smith")]
        [DataRow("surname=Smith||surname=Jones")]
        [DataRow("surname=Smith|$sort_by=amount")]
        [DataRow("surname=Smith|$page=2")]
        [DataRow("surname=Smith|amount")]
        public void Or_MalformedAlternative(string rest)
        {
            Assert.ThrowsException<ArgumentException>(() => parser.Parse(rest));
        }

        [TestMethod]
        public void Or_AlternativesCountTowardsConditionLimit()
        {
            string fifty = string.Join("|", Enumerable.Range(1, 50).Select(i => $"amount={i}"));
            parser.Parse(fifty);

            string fiftyOne = fifty + "|amount=51";
            Assert.ThrowsException<ArgumentException>(() => parser.Parse(fiftyOne));
        }

        [TestMethod]
        public void In_ValuesCountTowardsConditionLimit()
        {
            string fifty = "amount[in]=" + string.Join(",", Enumerable.Range(1, 50));
            parser.Parse(fifty);

            Assert.ThrowsException<ArgumentException>(() => parser.Parse(fifty + ",51"));
            Assert.ThrowsException<ArgumentException>(() => parser.Parse(fifty + "&surname=Smith"));
        }
    }
}
