using Microsoft.VisualStudio.TestTools.UnitTesting;
using REST_Parser;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestParserTests
{
    [TestClass]
    public class RestToSQLFilterTests
    {

        [DataTestMethod]
        [DataRow("surname[eq]=McArthur", "surname = 'McArthur'")]
        [DataRow("surname[eq]=Smith", "surname = 'Smith'")]
        [DataRow("firstname[eq]=John", "firstname = 'John'")]
        [DataRow("firstname[eq]=James", "firstname = 'James'")]
        [DataRow("price[eq]=5", "price = 5")]
        [DataRow("price[eq]=5.25", "price = 5.25")]

        public void Equals_Test(string rest, string expected)
        {
            // arrange
            RestToSQLParser parser = new RestToSQLParser();
            // act
            string sql = parser.Parse(rest);
            // assert
            Assert.AreEqual(expected, sql);
        }

        [DataTestMethod]
        [DataRow("surname[ne]=McArthur", "surname <> 'McArthur'")]
        [DataRow("surname[ne]=Smith", "surname <> 'Smith'")]
        [DataRow("firstname[ne]=John", "firstname <> 'John'")]
        [DataRow("firstname[ne]=James", "firstname <> 'James'")]
        [DataRow("price[ne]=5", "price <> 5")]
        [DataRow("price[ne]=5.25", "price <> 5.25")]
        public void NotEquals_Test(string rest, string expected)
        {
            // arrange
            RestToSQLParser parser = new RestToSQLParser();
            // act
            string sql = parser.Parse(rest);
            // assert
            Assert.AreEqual(expected, sql);
        }
    }
}
