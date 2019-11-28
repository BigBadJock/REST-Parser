﻿using Microsoft.VisualStudio.TestTools.UnitTesting;
using REST_Parser;
using REST_Parser.ExpressionGenerators;
using REST_Parser.ExpressionGenerators.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace RestParserTests
{
    [TestClass]
    public class RestToLinq_GreaterThan_Tests
    {

        IQueryable<TestItem> data;
        IStringExpressionGenerator<TestItem> stringExpressionGenerator;
        IIntExpressionGenerator<TestItem> intExpressionGenerator;
        IDateExpressionGenerator<TestItem> dateExpressionGenerator;
        IDoubleExpressionGenerator<TestItem> doubleExpressionGenerator;
        IDecimalExpressionGenerator<TestItem> decimalExpressionGenerator;
        IBooleanExpressionGenerator<TestItem> booleanExpressionGenerator;
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
        public void GT_Int()
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Amount > 5);

            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("amount[gt]=5");

            IQueryable<TestItem> selectedData = data;
            expressions.ForEach(delegate (Expression<Func<TestItem, bool>> where) {
                selectedData = selectedData.Where(where);
            });
            // assert
            Assert.AreEqual(2, selectedData.Count());
            foreach(TestItem item in selectedData)
            {
                Assert.IsTrue(item.Amount > 5);
            }
        }

        [TestMethod]
        public void GT_Double()
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Price > 5.25);

            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("price[gt]=5.25");

            IQueryable<TestItem> selectedData = data;
            expressions.ForEach(delegate (Expression<Func<TestItem, bool>> where) {
                selectedData = selectedData.Where(where);
            });
            // assert
            Assert.AreEqual(2, selectedData.Count());
            foreach (TestItem item in selectedData)
            {
                Assert.IsTrue(item.Price > 5.25);
            }
        }

        [TestMethod]
        public void GT_Decimal()
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Rate > 2.2m);

            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("rate[gt]=2.2");

            IQueryable<TestItem> selectedData = data;
            expressions.ForEach(delegate (Expression<Func<TestItem, bool>> where) {
                selectedData = selectedData.Where(where);
            });
            // assert
            Assert.AreEqual(2, selectedData.Count());
            foreach (TestItem item in selectedData)
            {
                Assert.IsTrue(item.Rate > 2.2m);
            }
        }

        [TestMethod]
        public void GT_Date()
        {
            // arrange
            List<Expression<Func<TestItem, bool>>> expected = new List<Expression<Func<TestItem, bool>>>();
            expected.Add(p => p.Birthday > Convert.ToDateTime("1968/01/01"));

            // act
            List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("birthday[gt]=1968-01-01");

            IQueryable<TestItem> selectedData = data;
            expressions.ForEach(delegate (Expression<Func<TestItem, bool>> where) {
                selectedData = selectedData.Where(where);
            });
            // assert
            Assert.AreEqual(2, selectedData.Count());
            foreach (TestItem item in selectedData)
            {
                Assert.IsTrue(item.Birthday > Convert.ToDateTime("1968/01/01"));
            }
        }

        [TestMethod]
        public void GT_String()
        {
            try
            {
                List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("surname[gt]=bob");
                Assert.Fail("Expected REST_InvalidOperatorException not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The REST request contained an invalid operator (gt) for field (surname)", ex.Message);
            }
        }

        [TestMethod]
        public void GT_Boolean()
        {
            try
            {
                List<Expression<Func<TestItem, bool>>> expressions = parser.Parse("flag[gt]=true");
                Assert.Fail("Expected REST_InvalidOperatorException not thrown");
            }
            catch (Exception ex)
            {
                Assert.AreEqual("The REST request contained an invalid operator (gt) for field (flag)", ex.Message);
            }
        }

    }
}
