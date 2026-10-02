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
    public class RestToLinq_AdditionalTypes_Tests
    {
        IQueryable<TestItem> data;
        RestToLinqParser<TestItem> parser;

        [TestInitialize]
        public void Initialize()
        {
            List<TestItem> d1 = new List<TestItem>();
            d1.Add(new TestItem
            {
                Id = 1, BigNumber = 5_000_000_000, NullableBigNumber = 10, Weight = 1.5f, Quantity = 10, Level = 1,
                Status = TestStatus.Draft, NullableStatus = TestStatus.Active,
                StartDate = new DateOnly(2024, 1, 1), EndDate = new DateOnly(2024, 1, 31), OpensAt = new TimeOnly(9, 0),
                Duration = TimeSpan.FromMinutes(30), NullableDuration = TimeSpan.FromHours(1)
            });
            d1.Add(new TestItem
            {
                Id = 2, BigNumber = -1, Weight = 2.25f, Quantity = -5, Level = 200,
                Status = TestStatus.Active,
                StartDate = new DateOnly(2024, 6, 15), OpensAt = new TimeOnly(12, 30),
                Duration = TimeSpan.FromDays(1)
            });
            d1.Add(new TestItem
            {
                Id = 3, BigNumber = 5_000_000_001, NullableBigNumber = 20, Weight = 0.5f, Quantity = 300, Level = 255,
                Status = TestStatus.Archived, NullableStatus = TestStatus.Archived,
                StartDate = new DateOnly(2025, 1, 1), EndDate = new DateOnly(2025, 2, 1), OpensAt = new TimeOnly(17, 45),
                Duration = new TimeSpan(2, 15, 0), NullableDuration = TimeSpan.FromMinutes(15)
            });
            this.data = d1.AsQueryable();

            this.parser = new RestToLinqParser<TestItem>(
                new StringExpressionGenerator<TestItem>(),
                new IntExpressionGenerator<TestItem>(),
                new DateExpressionGenerator<TestItem>(),
                new DoubleExpressionGenerator<TestItem>(),
                new DecimalExpressionGenerator<TestItem>(),
                new BooleanExpressionGenerator<TestItem>(),
                new GuidExpressionGenerator<TestItem>(),
                new DateTimeOffsetExpressionGenerator<TestItem>(),
                new NumericExpressionGenerator<TestItem>(),
                new EnumExpressionGenerator<TestItem>(),
                new DateOnlyExpressionGenerator<TestItem>(),
                new TimeOnlyExpressionGenerator<TestItem>(),
                new TimeSpanExpressionGenerator<TestItem>());
        }

        private int[] Ids(string rest)
        {
            return parser.Run(data, rest).Data.Select(p => p.Id).ToArray();
        }

        [TestMethod]
        [DataRow("bigNumber=5000000000", new[] { 1 })]
        [DataRow("bigNumber[ne]=-1", new[] { 1, 3 })]
        [DataRow("bigNumber[gt]=5000000000", new[] { 3 })]
        [DataRow("bigNumber[ge]=5000000000", new[] { 1, 3 })]
        [DataRow("bigNumber[lt]=0", new[] { 2 })]
        [DataRow("bigNumber[le]=5000000000", new[] { 1, 2 })]
        [DataRow("nullableBigNumber[ge]=10", new[] { 1, 3 })]
        [DataRow("nullableBigNumber[ne]=10", new[] { 2, 3 })]
        public void Long_Operators(string rest, int[] expectedIds)
        {
            CollectionAssert.AreEqual(expectedIds, Ids(rest));
        }

        [TestMethod]
        [DataRow("weight=1.5", new[] { 1 })]
        [DataRow("weight[gt]=1", new[] { 1, 2 })]
        [DataRow("weight[le]=1.5", new[] { 1, 3 })]
        [DataRow("quantity[lt]=0", new[] { 2 })]
        [DataRow("quantity[ge]=10", new[] { 1, 3 })]
        [DataRow("level=255", new[] { 3 })]
        [DataRow("level[gt]=100", new[] { 2, 3 })]
        public void FloatShortByte_Operators(string rest, int[] expectedIds)
        {
            CollectionAssert.AreEqual(expectedIds, Ids(rest));
        }

        [TestMethod]
        [DataRow("status=Active", new[] { 2 })]
        [DataRow("status[eq]=active", new[] { 2 })]
        [DataRow("status=2", new[] { 3 })]
        [DataRow("status[ne]=Draft", new[] { 2, 3 })]
        [DataRow("status[gt]=Draft", new[] { 2, 3 })]
        [DataRow("status[ge]=Active", new[] { 2, 3 })]
        [DataRow("status[lt]=archived", new[] { 1, 2 })]
        [DataRow("status[le]=Active", new[] { 1, 2 })]
        [DataRow("nullableStatus=Active", new[] { 1 })]
        [DataRow("nullableStatus[ge]=Active", new[] { 1, 3 })]
        [DataRow("nullableStatus[ne]=Archived", new[] { 1, 2 })]
        public void Enum_Operators(string rest, int[] expectedIds)
        {
            CollectionAssert.AreEqual(expectedIds, Ids(rest));
        }

        [TestMethod]
        [DataRow("startDate=2024-06-15", new[] { 2 })]
        [DataRow("startDate[ne]=2024-06-15", new[] { 1, 3 })]
        [DataRow("startDate[gt]=2024-06-15", new[] { 3 })]
        [DataRow("startDate[le]=2024-06-15", new[] { 1, 2 })]
        [DataRow("endDate[lt]=2025-01-01", new[] { 1 })]
        [DataRow("endDate[ne]=2024-01-31", new[] { 2, 3 })]
        public void DateOnly_Operators(string rest, int[] expectedIds)
        {
            CollectionAssert.AreEqual(expectedIds, Ids(rest));
        }

        [TestMethod]
        [DataRow("opensAt=09:00", new[] { 1 })]
        [DataRow("opensAt=17:45:00", new[] { 3 })]
        [DataRow("opensAt[ge]=12:30", new[] { 2, 3 })]
        [DataRow("opensAt[lt]=12:00", new[] { 1 })]
        public void TimeOnly_Operators(string rest, int[] expectedIds)
        {
            CollectionAssert.AreEqual(expectedIds, Ids(rest));
        }

        [TestMethod]
        [DataRow("duration=00:30:00", new[] { 1 })]
        [DataRow("duration=1.00:00:00", new[] { 2 })]
        [DataRow("duration[gt]=01:00:00", new[] { 2, 3 })]
        [DataRow("duration[le]=02:15", new[] { 1, 3 })]
        [DataRow("nullableDuration[lt]=01:00", new[] { 3 })]
        [DataRow("nullableDuration[ge]=00:15", new[] { 1, 3 })]
        public void TimeSpan_Operators(string rest, int[] expectedIds)
        {
            CollectionAssert.AreEqual(expectedIds, Ids(rest));
        }

        [TestMethod]
        [DataRow("bigNumber=abc")]
        [DataRow("level=256")]
        [DataRow("quantity=1.5")]
        [DataRow("status=Unknown")]
        [DataRow("startDate=2024-13-01")]
        [DataRow("opensAt=25:00")]
        [DataRow("duration=notatime")]
        public void InvalidValues(string rest)
        {
            Assert.ThrowsException<REST_InvalidValueException>(() => parser.Parse(rest));
        }

        [TestMethod]
        [DataRow("bigNumber[contains]=1")]
        [DataRow("status[contains]=Active")]
        [DataRow("startDate[contains]=2024-01-01")]
        [DataRow("opensAt[contains]=09:00")]
        [DataRow("duration[contains]=01:00")]
        public void InvalidOperators(string rest)
        {
            Assert.ThrowsException<REST_InvalidOperatorException>(() => parser.Parse(rest));
        }

        [TestMethod]
        public void SevenArgumentConstructorUsesDefaultGenerators()
        {
            var legacyParser = new RestToLinqParser<TestItem>(
                new StringExpressionGenerator<TestItem>(),
                new IntExpressionGenerator<TestItem>(),
                new DateExpressionGenerator<TestItem>(),
                new DoubleExpressionGenerator<TestItem>(),
                new DecimalExpressionGenerator<TestItem>(),
                new BooleanExpressionGenerator<TestItem>(),
                new GuidExpressionGenerator<TestItem>());

            CollectionAssert.AreEqual(new[] { 2, 3 }, legacyParser.Run(data, "status[gt]=Draft&bigNumber[ne]=5000000000").Data.Select(p => p.Id).ToArray());
        }
    }
}
