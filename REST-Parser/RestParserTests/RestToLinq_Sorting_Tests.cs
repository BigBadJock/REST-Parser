using Microsoft.VisualStudio.TestTools.UnitTesting;
using REST_Parser;
using REST_Parser.ExpressionGenerators;
using REST_Parser.ExpressionGenerators.Interfaces;
using REST_Parser.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace RestParserTests
{
    [TestClass]
    public class RestToLinq_Sorting_Tests
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



        [TestMethod]
        public void SORT_String_Ascending()
        {
            // arrange
            string rest = "id[gt]=0&$sort_by[asc]=surname";
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Id > 0);

            // act
            IQueryable<TestItem> selectedData = parser.Run(this.data, rest);

            // assert
            Assert.AreEqual(4, selectedData.Count());
            string previous = "";
            selectedData.ToList<TestItem>().ForEach(delegate(TestItem testItem)
            {
                Assert.IsTrue(testItem.Surname.CompareTo(previous) > -1);
                previous = testItem.Surname;
            });


        }

        [TestMethod]
        public void SORT_String_Descending()
        {
            // arrange
            string rest = "id[gt]=0&$sort_by[desc]=surname";
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Id > 0);

            // act
            IQueryable<TestItem> selectedData = parser.Run(this.data, rest);

            // assert
            Assert.AreEqual(4, selectedData.Count());
            string previous = "ZZZZZ";
            selectedData.ToList<TestItem>().ForEach(delegate (TestItem testItem)
            {
                Assert.IsTrue(testItem.Surname.CompareTo(previous) < 1);
                previous = testItem.Surname;
            });


        }

        [TestMethod]
        public void SORT_Integer_Ascending()
        {
            // arrange
            string rest = "id[gt]=0&$sort_by[asc]=amount";
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Id > 0);

            // act
            IQueryable<TestItem> selectedData = parser.Run(this.data, rest);

            // assert
            Assert.AreEqual(4, selectedData.Count());
            int previous = 0;
            selectedData.ToList<TestItem>().ForEach(delegate (TestItem testItem)
            {
                Assert.IsTrue(testItem.Amount.CompareTo(previous) > -1);
                previous = testItem.Amount;
            });
        }

        [TestMethod]
        public void SORT_Integer_Descending()
        {
            // arrange
            string rest = "id[gt]=0&$sort_by[desc]=amount";
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Id > 0);

            // act
            IQueryable<TestItem> selectedData = parser.Run(this.data, rest);
            // assert
            Assert.AreEqual(4, selectedData.Count());
            int previous = 999;
            selectedData.ToList<TestItem>().ForEach(delegate (TestItem testItem)
            {
                Assert.IsTrue(testItem.Amount.CompareTo(previous) < 1);
                previous = testItem.Amount;
            });
        }


        [TestMethod]
        public void SORT_Double_Ascending()
        {
            // arrange
            string rest = "id[gt]=0&$sort_by[asc]=price";
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Id > 0);

            // act
            IQueryable<TestItem> selectedData = parser.Run(this.data, rest);

            // assert
            Assert.AreEqual(4, selectedData.Count());
            double previous = 0;
            selectedData.ToList<TestItem>().ForEach(delegate (TestItem testItem)
            {
                Assert.IsTrue(testItem.Price.CompareTo(previous) > -1);
                previous = testItem.Price;
            });
        }

        [TestMethod]
        public void SORT_Double_Descending()
        {
            // arrange
            string rest = "id[gt]=0&$sort_by[desc]=price";
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Id > 0);

            // act
            IQueryable<TestItem> selectedData = parser.Run(this.data, rest);
            // assert
            Assert.AreEqual(4, selectedData.Count());
            double previous = 999;
            selectedData.ToList<TestItem>().ForEach(delegate (TestItem testItem)
            {
                Assert.IsTrue(testItem.Price.CompareTo(previous) < 1);
                previous = testItem.Price;
            });
        }

        [TestMethod]
        public void SORT_Decimal_Ascending()
        {
            // arrange
            string rest = "id[gt]=0&$sort_by[asc]=rate";
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Id > 0);

            // act
            IQueryable<TestItem> selectedData = parser.Run(this.data, rest);

            // assert
            Assert.AreEqual(4, selectedData.Count());
            decimal previous = 0m;
            selectedData.ToList<TestItem>().ForEach(delegate (TestItem testItem)
            {
                Assert.IsTrue(testItem.Rate.CompareTo(previous) > -1);
                previous = testItem.Rate;
            });
        }

        [TestMethod]
        public void SORT_Decimal_Descending()
        {
            // arrange
            string rest = "id[gt]=0&$sort_by[desc]=rate";
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Id > 0);

            // act
            IQueryable<TestItem> selectedData = parser.Run(this.data, rest);
            // assert
            Assert.AreEqual(4, selectedData.Count());
            decimal previous = 999m;
            selectedData.ToList<TestItem>().ForEach(delegate (TestItem testItem)
            {
                Assert.IsTrue(testItem.Rate.CompareTo(previous) < 1);
                previous = testItem.Rate;
            });
        }

        [TestMethod]
        public void SORT_Date_Ascending()
        {
            // arrange
            string rest = "id[gt]=0&$sort_by[asc]=birthday";
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Id > 0);

            // act
            IQueryable<TestItem> selectedData = parser.Run(this.data, rest);

            // assert
            Assert.AreEqual(4, selectedData.Count());
            DateTime previous = DateTime.MinValue;
            selectedData.ToList<TestItem>().ForEach(delegate (TestItem testItem)
            {
                Assert.IsTrue(testItem.Birthday.CompareTo(previous) > -1);
                previous = testItem.Birthday;
            });
        }

        [TestMethod]
        public void SORT_Date_Descending()
        {
            // arrange
            string rest = "id[gt]=0&$sort_by[desc]=birthday";
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Id > 0);

            // act
            IQueryable<TestItem> selectedData = parser.Run(this.data, rest);
            // assert
            Assert.AreEqual(4, selectedData.Count());
            DateTime previous = DateTime.MaxValue;
            selectedData.ToList<TestItem>().ForEach(delegate (TestItem testItem)
            {
                Assert.IsTrue(testItem.Birthday.CompareTo(previous) < 1);
                previous = testItem.Birthday;
            });
        }

        [TestMethod]
        public void SORT_Boolean_Ascending()
        {
            // arrange
            string rest = "id[gt]=0&$sort_by[asc]=flag";
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Id > 0);

            // act
            IQueryable<TestItem> selectedData = parser.Run(this.data, rest);

            // assert
            Assert.AreEqual(4, selectedData.Count());
            bool previous = false;
            selectedData.ToList<TestItem>().ForEach(delegate (TestItem testItem)
            {
                Assert.IsTrue(testItem.Flag.CompareTo(previous) > -1);
                previous = testItem.Flag;
            });
        }

        [TestMethod]
        public void SORT_Boolean_Descending()
        {
            // arrange
            string rest = "id[gt]=0&$sort_by[desc]=flag";
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Id > 0);

            // act
            IQueryable<TestItem> selectedData = parser.Run(this.data, rest);
            // assert
            Assert.AreEqual(4, selectedData.Count());
            bool previous = true;
            selectedData.ToList<TestItem>().ForEach(delegate (TestItem testItem)
            {
                Assert.IsTrue(testItem.Flag.CompareTo(previous) < 1);
                previous = testItem.Flag;
            });
        }

        [TestMethod]
        public void SORT_string_Ascending_then_string_ascending()
        {
            // arrange
            string rest = "id[gt]=0&$sort_by[asc]=surname&$sort_by[asc]=firstname";
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Id > 0);

            // act
            IQueryable<TestItem> selectedData = parser.Run(this.data, rest);

            // assert
            Assert.AreEqual(4, selectedData.Count());
            string surname= "";
            string firstname = "";
            selectedData.ToList<TestItem>().ForEach(delegate (TestItem testItem)
            {
                if(testItem.Surname == surname)
                {
                    Assert.IsTrue(testItem.FirstName.CompareTo(firstname) > -1,"firstname should be greater");
                }
                Assert.IsTrue(testItem.Surname.CompareTo(surname) > -1, "Surname should be greater");
                surname = testItem.Surname;
                firstname = testItem.FirstName;
            });
        }

        [TestMethod]
        public void SORT_string_Ascending_then_string_descending()
        {
            // arrange
            string rest = "id[gt]=0&$sort_by[asc]=surname&$sort_by[desc]=firstname";
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Id > 0);

            // act
            IQueryable<TestItem> selectedData = parser.Run(this.data, rest);

            // assert
            Assert.AreEqual(4, selectedData.Count());
            string surname = "";
            string firstname = "ZZZ";
            selectedData.ToList<TestItem>().ForEach(delegate (TestItem testItem)
            {
                if (testItem.Surname == surname)
                {
                    Assert.IsTrue(testItem.FirstName.CompareTo(firstname) <1, "firstname should be smaller");
                }
                Assert.IsTrue(testItem.Surname.CompareTo(surname) > -1, "Surname should be greater");
                surname = testItem.Surname;
                firstname = testItem.FirstName;
            });
        }

    }
}
