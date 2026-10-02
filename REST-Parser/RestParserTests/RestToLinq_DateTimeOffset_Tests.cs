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
    public class RestToLinq_DateTimeOffset_Tests
    {
        IQueryable<TestItem> data;
        RestToLinqParser<TestItem> parser;

        [TestInitialize]
        public void Initialize()
        {
            List<TestItem> d1 = new List<TestItem>();
            d1.Add(new TestItem { Id = 1, CreatedAt = new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), ShippedAt = new DateTimeOffset(2024, 1, 3, 0, 0, 0, TimeSpan.Zero) });
            d1.Add(new TestItem { Id = 2, CreatedAt = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.Zero) });
            d1.Add(new TestItem { Id = 3, CreatedAt = new DateTimeOffset(2024, 6, 1, 12, 0, 0, TimeSpan.FromHours(2)), ShippedAt = new DateTimeOffset(2024, 6, 2, 0, 0, 0, TimeSpan.Zero) });
            d1.Add(new TestItem { Id = 4, CreatedAt = new DateTimeOffset(2025, 1, 1, 0, 0, 0, TimeSpan.Zero) });
            this.data = d1.AsQueryable();

            this.parser = new RestToLinqParser<TestItem>(
                new StringExpressionGenerator<TestItem>(),
                new IntExpressionGenerator<TestItem>(),
                new DateExpressionGenerator<TestItem>(),
                new DoubleExpressionGenerator<TestItem>(),
                new DecimalExpressionGenerator<TestItem>(),
                new BooleanExpressionGenerator<TestItem>(),
                new GuidExpressionGenerator<TestItem>(),
                new DateTimeOffsetExpressionGenerator<TestItem>());
        }

        private int[] Ids(string rest)
        {
            return parser.Run(data, rest).Data.Select(p => p.Id).ToArray();
        }

        [TestMethod]
        [DataRow("createdAt[eq]=2024-01-01T00:00:00Z", new[] { 1 })]
        [DataRow("createdAt=2024-01-01", new[] { 1 })]
        [DataRow("createdAt[ne]=2024-01-01T00:00:00Z", new[] { 2, 3, 4 })]
        [DataRow("createdAt[gt]=2024-06-01T12:00:00Z", new[] { 4 })]
        [DataRow("createdAt[ge]=2024-06-01T12:00:00Z", new[] { 2, 4 })]
        [DataRow("createdAt[lt]=2024-06-01T12:00:00Z", new[] { 1, 3 })]
        [DataRow("createdAt[le]=2024-06-01T12:00:00Z", new[] { 1, 2, 3 })]
        public void DateTimeOffset_Operators(string rest, int[] expectedIds)
        {
            CollectionAssert.AreEqual(expectedIds, Ids(rest));
        }

        [TestMethod]
        public void DateTimeOffset_ComparesByInstantAcrossOffsets()
        {
            // 12:00+02:00 is 10:00Z
            CollectionAssert.AreEqual(new[] { 3 }, Ids("createdAt[eq]=2024-06-01T10:00:00Z"));
        }

        [TestMethod]
        public void DateTimeOffset_ValueWithoutOffsetIsUtc()
        {
            CollectionAssert.AreEqual(new[] { 3 }, Ids("createdAt[eq]=2024-06-01T10:00:00"));
        }

        [TestMethod]
        [DataRow("shippedAt[eq]=2024-01-03", new[] { 1 })]
        [DataRow("shippedAt[ne]=2024-01-03", new[] { 2, 3, 4 })]
        [DataRow("shippedAt[gt]=2024-01-03", new[] { 3 })]
        [DataRow("shippedAt[le]=2024-06-02", new[] { 1, 3 })]
        public void NullableDateTimeOffset_Operators(string rest, int[] expectedIds)
        {
            CollectionAssert.AreEqual(expectedIds, Ids(rest));
        }

        [TestMethod]
        public void DateTimeOffset_InvalidValue()
        {
            Assert.ThrowsException<REST_InvalidValueException>(() => parser.Parse("createdAt[eq]=notadate"));
        }

        [TestMethod]
        public void DateTimeOffset_InvalidOperator()
        {
            Assert.ThrowsException<REST_InvalidOperatorException>(() => parser.Parse("createdAt[contains]=2024-01-01"));
        }

        [TestMethod]
        public void DateTimeOffset_SevenArgumentConstructorStillFilters()
        {
            var legacyParser = new RestToLinqParser<TestItem>(
                new StringExpressionGenerator<TestItem>(),
                new IntExpressionGenerator<TestItem>(),
                new DateExpressionGenerator<TestItem>(),
                new DoubleExpressionGenerator<TestItem>(),
                new DecimalExpressionGenerator<TestItem>(),
                new BooleanExpressionGenerator<TestItem>(),
                new GuidExpressionGenerator<TestItem>());

            CollectionAssert.AreEqual(new[] { 4 }, legacyParser.Run(data, "createdAt[gt]=2024-12-31").Data.Select(p => p.Id).ToArray());
        }
    }
}
