using Microsoft.VisualStudio.TestTools.UnitTesting;
using REST_Parser;
using REST_Parser.ExpressionGenerators;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace RestParserTests
{
    [TestClass]
    public class RestToLinq_NullableFields_Tests
    {
        IQueryable<TestItem> data;
        IStringExpressionGenerator<TestItem> stringExpressionGenerator;
        IIntExpressionGenerator<TestItem> intExpressionGenerator;
        IDateExpressionGenerator<TestItem> dateExpressionGenerator;
        private DoubleExpressionGenerator<TestItem> doubleExpressionGenerator;
        private DecimalExpressionGenerator<TestItem> decimalExpressionGenerator;
        private BooleanExpressionGenerator<TestItem> booleanExpressionGenerator;
        private GuidExpressionGenerator<TestItem> guidExpressionGenerator;

        RestToLinqParser<TestItem> parser;

        [TestInitialize]
        public void Initialize()
        {
            List<TestItem> d1 = new List<TestItem>();
            d1.Add(new TestItem { Id = 1, FirstName = "Bob", Surname = "Roberts", Amount = 4, Price = 4, Rate = 2.1m, Birthday = Convert.ToDateTime("1966/01/01"), Flag = true, OrderCount = 2, OrderWeight = 2.5, OrderCost = 25 });
            d1.Add(new TestItem { Id = 2, FirstName = "John", Surname = "McArthur", Amount = 5, Price = 5.25, Rate = 2.2m, Birthday = Convert.ToDateTime("1968/01/01"), Flag = false });
            d1.Add(new TestItem { Id = 3, FirstName = "James", Surname = "McArthur", Amount = 6, Price = 5.5, Rate = 2.3m, Birthday = Convert.ToDateTime("1970/01/01"), Flag = true, OrderCount = 3, OrderWeight = 3.75, OrderCost = 50, MarriageDate=Convert.ToDateTime("2005/10/28" ) });
            d1.Add(new TestItem { Id = 4, FirstName = "James", MiddleName = "Joseph", Surname = "Smith", Amount = 7, Price = 6.5, Rate = 2.4m, Birthday = Convert.ToDateTime("1972/01/01"), Flag = true });
            this.data = d1.AsQueryable();

            this.stringExpressionGenerator = new StringExpressionGenerator<TestItem>();
            this.intExpressionGenerator = new IntExpressionGenerator<TestItem>();
            this.dateExpressionGenerator = new DateExpressionGenerator<TestItem>();
            this.doubleExpressionGenerator = new DoubleExpressionGenerator<TestItem>();
            this.decimalExpressionGenerator = new DecimalExpressionGenerator<TestItem>();
            this.booleanExpressionGenerator = new BooleanExpressionGenerator<TestItem>();
            this.guidExpressionGenerator = new GuidExpressionGenerator<TestItem>();
            this.parser = new RestToLinqParser<TestItem>(stringExpressionGenerator, intExpressionGenerator, dateExpressionGenerator, doubleExpressionGenerator, decimalExpressionGenerator, booleanExpressionGenerator, guidExpressionGenerator);
        }

        [DataTestMethod]
        [DataRow("orderCount[eq]=2", 1)]
        [DataRow("orderCount[eq]=3", 3)]
        [DataRow("orderCount[gt]=2", 3)]
        [DataRow("orderWeight[eq]=2.5", 1)]
        [DataRow("orderWeight[eq]=3.75", 3)]
        [DataRow("orderWeight[gt]=3", 3)]
        [DataRow("orderWeight[lt]=3", 1)]
        [DataRow("middleName[eq]=Joseph", 4)]
//        [DataRow("middleName[contains]=Jo", 4)]
        [DataRow("ordercost[eq]=25", 1)]
        [DataRow("ordercost[eq]=50", 3)]
        [DataRow("ordercost[gt]=30", 3)]
        [DataRow("MarriageDate[gt]=2005/01/01", 3)]
        [DataRow("MarriageDate[eq]=2005/10/28", 3)]
        public void Nullable_Test(string rest, int id)
        {
            // arrange
            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse(rest).Expressions;

            IQueryable<TestItem> selectedData = data;
            expressions.ForEach(delegate (Expression<Func<TestItem, bool>> where)
            {
                selectedData = selectedData.Where(where);
            });
            TestItem first = selectedData.FirstOrDefault();
            // assert
            Assert.AreEqual(1, selectedData.Count());

            Assert.AreEqual(id, first.Id);
        }

    }
}
