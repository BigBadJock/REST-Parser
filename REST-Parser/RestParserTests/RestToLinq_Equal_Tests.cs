using Microsoft.VisualStudio.TestTools.UnitTesting;
using REST_Parser;
using REST_Parser.Exceptions;
using REST_Parser.ExpressionGenerators;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RestParserTests
{
    [TestClass]
    public class RestToLinq_Equal_Tests
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
            d1.Add(new TestItem { Id = 1, FirstName = "Bob", Surname = "Roberts", Amount = 4, Price = 4, Rate=2.1m, Birthday=Convert.ToDateTime("1966/01/01"), Flag=true});
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
        [DataRow("surname[eq]=Roberts", "Roberts")]
        [DataRow("surname[eq]=Smith", "Smith")]
        [DataRow("surname [eq] = Smith", "Smith")]
        [DataRow("surname=Smith", "Smith")]
        public void EQ_String_Test(string rest, string surname )
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
             expected.Add(p => p.Surname == surname);

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

        [DataTestMethod]
        [DataRow("surname[eq]=Smith&firstname[eq]=James", "Smith", "James")]
        [DataRow("surname[eq]=McArthur&firstname[eq]=John", "McArthur", "John")]
        [DataRow("surname[eq]=McArthur&firstname[eq]=James", "McArthur", "James")]
        [DataRow("surname [eq] = McArthur & firstname [eq] = James", "McArthur", "James")]
        [DataRow("surname=McArthur&firstname=James", "McArthur", "James")]
        public void EQ_Multiple_Test(string rest, string surname, string firstname)
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Surname == surname);
            expected.Add(p => p.FirstName == firstname);

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
            Assert.AreEqual(firstname, first.FirstName);
        }

        [DataTestMethod]
        [DataRow("amount[eq]=5", 5, "McArthur")]
        [DataRow("amount[eq]=7", 7, "Smith")]
        [DataRow("amount=7",7, "Smith")]
        public void EQ_INT_Test(string rest, int amount, string surname)
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Amount == amount);

            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse(rest).Expressions;

            IQueryable<TestItem> selectedData = data;
            expressions.ForEach(delegate (Expression<Func<TestItem, bool>> where)
            {
                selectedData = selectedData.Where(where);
            });
            TestItem first = selectedData.FirstOrDefault();
            // assert
            Assert.AreEqual(expected.Count, expressions.Count);
            Assert.AreEqual(1, selectedData.Count());

            Assert.AreEqual(surname, first.Surname);
        }

        [DataTestMethod]
        [DataRow("surname[eq]=McArthur&amount[eq]=5", 5, "McArthur")]
        [DataRow("surname[eq]=Smith&amount[eq]=7", 7, "Smith")]
        public void EQ_Multiple_Mixed_INT_Test(string rest, int amount, string surname)
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Surname == surname);
            expected.Add(p => p.Amount == amount);

            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse(rest).Expressions;

            IQueryable<TestItem> selectedData = data;
            expressions.ForEach(delegate (Expression<Func<TestItem, bool>> where)
            {
                selectedData = selectedData.Where(where);
            });
            TestItem first = selectedData.FirstOrDefault();
            // assert
            Assert.AreEqual(expected.Count, expressions.Count);
            Assert.AreEqual(1, selectedData.Count());

            Assert.AreEqual(surname, first.Surname);
        }

        [TestMethod]
        [ExpectedException(typeof(REST_InvalidFieldnameException))]
        public void EQ_NonExistantField()
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Surname == "McArthur");

            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("lastname[eq]=McArthur").Expressions;

            // assert -expects exception
        }

        [DataTestMethod]
        [DataRow("birthday[eq]=1968-01-01", "1968/01/01", 2)]
        [DataRow("birthday[eq]=1972-01-01", "1972/01/01", 4)]
        [DataRow("birthday=1968-01-01", "1968/01/01", 2)]
        public void EQ_Date(string rest, string date, int id)
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Birthday == Convert.ToDateTime(date));

            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse(rest).Expressions;

            IQueryable<TestItem> selectedData = data;
            expressions.ForEach(delegate (Expression<Func<TestItem, bool>> where)
            {
                selectedData = selectedData.Where(where);
            });

            TestItem first = selectedData.FirstOrDefault();

            // assert
            Assert.AreEqual(id, first.Id);
        }

        [DataTestMethod]
        [DataRow("price[eq]=5.25",5.25,2)]
        [DataRow("price[eq]=5.5", 5, 3)]
        [DataRow("price=5.5", 5, 3)]
        public void EQ_Double(string rest, double price, int id)
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Price == price);

            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse(rest).Expressions;

            IQueryable<TestItem> selectedData = data;
            expressions.ForEach(delegate (Expression<Func<TestItem, bool>> where)
            {
                selectedData = selectedData.Where(where);
            });

            TestItem first = selectedData.FirstOrDefault();

            // assert
            Assert.AreEqual(id, first.Id);
        }

        [DataTestMethod]
        [DataRow("rate[eq]=2.1", 2.1, 1)]
        [DataRow("rate[eq]=2.3", 2.3, 3)]
        [DataRow("rate=2.3", 2.3, 3)]
        // passing in doubles as mstest doesn't allow passing in of decimals 
        public void EQ_Decimal(string rest, double rate, int id)
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Rate == Convert.ToDecimal(rate));

            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse(rest).Expressions;

            IQueryable<TestItem> selectedData = data;
            expressions.ForEach(delegate (Expression<Func<TestItem, bool>> where)
            {
                selectedData = selectedData.Where(where);
            });

            TestItem first = selectedData.FirstOrDefault();

            // assert
            Assert.AreEqual(id, first.Id);
        }

        [TestMethod]
        public void EQ_Boolean_true()
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Flag == true);

            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("flag[eq]=true").Expressions;

            IQueryable<TestItem> selectedData = data;
            expressions.ForEach(delegate (Expression<Func<TestItem, bool>> where)
            {
                selectedData = selectedData.Where(where);
            });

            int count = selectedData.Count();

            // assert
            Assert.AreEqual(3, count);

        }

        [TestMethod]
        public void EQ_Boolean_false()
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Flag == true);

            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("flag[eq]=false").Expressions;

            IQueryable<TestItem> selectedData = data;
            expressions.ForEach(delegate (Expression<Func<TestItem, bool>> where)
            {
                selectedData = selectedData.Where(where);
            });

            int count = selectedData.Count();

            // assert
            Assert.AreEqual(1, count);
        }

    }
}
