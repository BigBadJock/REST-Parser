using Microsoft.VisualStudio.TestTools.UnitTesting;
using REST_Parser;
using REST_Parser.ExpressionGenerators;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Linq;
using System.Linq.Expressions;
using REST_Parser.Exceptions;

namespace RestParserTests
{
    [TestClass]
    public class RestToLinq_InvalidValues_Tests
    {
        IQueryable<TestItem> data;
        IStringExpressionGenerator<TestItem> stringExpressionGenerator;
        IIntExpressionGenerator<TestItem> intExpressionGenerator;
        IDateExpressionGenerator<TestItem> dateExpressionGenerator;
        private DoubleExpressionGenerator<TestItem> doubleExpressionGenerator;
        private DecimalExpressionGenerator<TestItem> decimalExpressionGenerator;
        private BooleanExpressionGenerator<TestItem> booleanExpressionGenerator;
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

        [TestMethod]
        public void Invalid_Date_value()
        {
            try
            {
                List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("birthday[eq]=false").Expressions;
                Assert.Fail("Expected REST_InvalidValueException not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The REST request contained an invalid value (false) for field (birthday)", ex.Message);
            }
        }

        [TestMethod]
        public void Invalid_Int_value()
        {
            try
            {
                List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("Amount[eq]=bob").Expressions;
                Assert.Fail("Expected REST_InvalidValueException not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The REST request contained an invalid value (bob) for field (Amount)", ex.Message);
            }

        }

        [TestMethod]
        public void Invalid_Bool_value()
        {
            try
            {
                List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("Flag[eq]=bob").Expressions;
                Assert.Fail("Expected REST_InvalidValueException not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The REST request contained an invalid value (bob) for field (Flag)", ex.Message);
            }
        }

        [TestMethod]
        public void Invalid_Decimal_value()
        {
            try
            {
                List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("Rate[eq]=bob").Expressions;
                Assert.Fail("Expected REST_InvalidValueException not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The REST request contained an invalid value (bob) for field (Rate)", ex.Message);
            }
        }

        [TestMethod]
        public void Invalid_Double_value()
        {

            try
            {
                List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("Price[eq]=bob").Expressions;
                Assert.Fail("Expected REST_InvalidValueException not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The REST request contained an invalid value (bob) for field (Price)", ex.Message);
            }
        }
    }
}
