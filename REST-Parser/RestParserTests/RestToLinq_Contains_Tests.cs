using Microsoft.VisualStudio.TestTools.UnitTesting;
using REST_Parser;
using REST_Parser.ExpressionGenerators;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RestParserTests
{

    [TestClass]
    public class RestToLinq_Contains_Tests
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
        [DataRow("surname[contains]=Rob", "Roberts")]
        [DataRow("surname[contains]=mith", "Smith")]
        public void Contains_String_Test(string rest, string surname)
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Surname.Contains(surname));

            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse(rest).Expressions;

            IQueryable<TestItem> selectedData = data;
            expressions.ForEach(delegate (Expression<Func<TestItem, bool>> where) {
                selectedData = selectedData.Where(where);
            });
            TestItem first = selectedData.FirstOrDefault();
            // assert
            Assert.AreEqual(expected.Count, expressions.Count);
            Assert.AreEqual(1, selectedData.Count());

            Assert.AreEqual(surname, first.Surname);
        }

        [TestMethod]
        public void Contains_Int_Test()
        {
            try
            {
                List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("amount[contains]=5").Expressions;
                Assert.Fail("Expected REST_InvalidOperatorException not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The REST request contained an invalid operator (contains) for field (amount)", ex.Message);
            }
        }

        [TestMethod]
        public void Contains_Double_Test()
        {
            try
            {
                List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("price[contains]=5").Expressions;
                Assert.Fail("Expected REST_InvalidOperatorException not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The REST request contained an invalid operator (contains) for field (price)", ex.Message);
            }
        }

        [TestMethod]
        public void Contains_Decimal_Test()
        {
            try
            {
                List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("rate[contains]=5").Expressions;
                Assert.Fail("Expected REST_InvalidOperatorException not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The REST request contained an invalid operator (contains) for field (rate)", ex.Message);
            }
        }

        [TestMethod]
        public void Contains_Date_Test()
        {
            try
            {
                List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("birthday[contains]=2019/01/01").Expressions;
                Assert.Fail("Expected REST_InvalidOperatorException not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The REST request contained an invalid operator (contains) for field (birthday)", ex.Message);
            }
        }

        [TestMethod]
        public void Contains_Boolean_Test()
        {
            try
            {
                List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("flag[contains]=true").Expressions;
                Assert.Fail("Expected REST_InvalidOperatorException not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The REST request contained an invalid operator (contains) for field (flag)", ex.Message);
            }
        }

    }
}
