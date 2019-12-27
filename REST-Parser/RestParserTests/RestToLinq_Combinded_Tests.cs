using Microsoft.VisualStudio.TestTools.UnitTesting;
using REST_Parser;
using REST_Parser.ExpressionGenerators;
using REST_Parser.ExpressionGenerators.Interfaces;
using REST_Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestParserTests
{
    [TestClass]
   public class RestToLinq_Combinded_Tests
    {
        IQueryable<TestItem> data;
        private IStringExpressionGenerator<TestItem> stringExpressionGenerator;
        private IIntExpressionGenerator<TestItem> intExpressionGenerator;
        private IDateExpressionGenerator<TestItem> dateExpressionGenerator;
        private IDoubleExpressionGenerator<TestItem> doubleExpressionGenerator;
        private IDecimalExpressionGenerator<TestItem> decimalExpressionGenerator;
        private IBooleanExpressionGenerator<TestItem> booleanExpressionGenerator;
        RestToLinqParser<TestItem> parser;

        [TestInitialize]
        public void Initialize()
        {
            List<TestItem> d1 = new List<TestItem>();
            d1.Add(new TestItem { Id = 1, FirstName = "Bob", Surname = "Roberts", Amount = 4, Price = 4, Rate = 2.1m, Birthday = Convert.ToDateTime("1966/01/01"), Flag = true });
            d1.Add(new TestItem { Id = 2, FirstName = "John", Surname = "McArthur", Amount = 5, Price = 5.25, Rate = 2.2m, Birthday = Convert.ToDateTime("1968/01/01"), Flag = false });
            d1.Add(new TestItem { Id = 3, FirstName = "James", Surname = "McArthur", Amount = 6, Price = 5.5, Rate = 2.3m, Birthday = Convert.ToDateTime("1970/01/01"), Flag = true });
            d1.Add(new TestItem { Id = 4, FirstName = "James", Surname = "Smith", Amount = 7, Price = 6.5, Rate = 2.4m, Birthday = Convert.ToDateTime("1972/01/01"), Flag = true });
            this.data = d1.AsQueryable();

            this.stringExpressionGenerator = new StringExpressionGenerator<TestItem>();
            this.intExpressionGenerator = new IntExpressionGenerator<TestItem>();
            this.dateExpressionGenerator = new DateExpressionGenerator<TestItem>();
            this.doubleExpressionGenerator = new DoubleExpressionGenerator<TestItem>();
            this.decimalExpressionGenerator = new DecimalExpressionGenerator<TestItem>();
            this.booleanExpressionGenerator = new BooleanExpressionGenerator<TestItem>();
            this.parser = new RestToLinqParser<TestItem>(stringExpressionGenerator, intExpressionGenerator, dateExpressionGenerator, doubleExpressionGenerator, decimalExpressionGenerator, booleanExpressionGenerator);
        }


        [DataTestMethod]
        [DataRow("surname[eq]=McArthur&$sort_by[asc]=firstname",2)]
        [DataRow("firstname[eq]=James&$sort_by[desc]=amount", 2)]
        [DataRow("$sort_by[desc]=amount&$pageSize=2&$page=2", 2)]
        [DataRow("$sort_by[asc]=birthday&$pageSize=3&$page=2", 1)]
        [DataRow("",4)]
        public void FullTest(string rest, int expectedCount)
        {
            // arrange
            
            // act
            RestResult<TestItem> result = parser.Run(this.data, rest);

            // assert
            Assert.AreEqual(expectedCount, result.Data.Count());
        }

    }
}
