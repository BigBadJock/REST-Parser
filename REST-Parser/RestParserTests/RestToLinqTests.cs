using Microsoft.VisualStudio.TestTools.UnitTesting;
using REST_Parser;
using REST_Parser.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RestParserTests
{
    [TestClass]
    public class RestToLinqTests
    {
        IQueryable<TestItem> data;

        [TestInitialize]
        public void Initialize()
        {
            List<TestItem> d1 = new List<TestItem>();
            d1.Add(new TestItem { FirstName = "Bob", Surname = "Roberts", Amount = 4, Price = 4 });
            d1.Add(new TestItem { FirstName = "John", Surname = "McArthur", Amount = 5, Price = 5.25 });
            d1.Add(new TestItem { FirstName = "James", Surname = "McArthur", Amount = 6, Price = 5.5 });
            d1.Add(new TestItem { FirstName = "James", Surname = "Smith", Amount = 7, Price = 6.5 });
            this.data = d1.AsQueryable();
        }

        [DataTestMethod]
        [DataRow("surname[eq]=Roberts", "Roberts")]
        [DataRow("surname[eq]=Smith", "Smith")]
        [DataRow("surname [eq] = Smith", "Smith")]
        public void Equals_Test(string rest, string surname )
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
             expected.Add(p => p.Surname == surname);

            RestToLinqParser<TestItem> parser = new RestToLinqParser<TestItem>();
            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse(rest);

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
        public void Multiple_Equals_Test(string rest, string surname, string firstname)
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Surname == surname);
            expected.Add(p => p.FirstName == firstname);

            RestToLinqParser<TestItem> parser = new RestToLinqParser<TestItem>();
            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse(rest);

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
        public void Equals_INT_Test(string rest, int amount, string surname)
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Amount == amount);

            RestToLinqParser<TestItem> parser = new RestToLinqParser<TestItem>();
            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse(rest);

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
        public void Multiple_Mixed_Equals_INT_Test(string rest, int amount, string surname)
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Surname == surname);
            expected.Add(p => p.Amount == amount);

            RestToLinqParser<TestItem> parser = new RestToLinqParser<TestItem>();
            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse(rest);

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
        [ExpectedException(typeof(InvalidRestException))]
        public void EQ_NonExistantField()
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Surname == "McArthur");
            RestToLinqParser<TestItem> parser = new RestToLinqParser<TestItem>();

            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("lastname[eq]=McArthur");

            // assert -expects exception
            

        }


    }
}
