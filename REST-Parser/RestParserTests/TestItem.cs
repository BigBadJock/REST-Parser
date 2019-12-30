using System;

namespace RestParserTests
{
    internal class TestItem
    {
        public int Id { get; set; }
        public string Surname { get; set; }
        public string FirstName { get; set; }
        public int Amount { get; set; }
        public double Price { get; set; }
        public decimal Rate { get; set; }
        public DateTime Birthday { get; set; }
        public bool Flag { get; set; }
        public string? MiddleName { get; set; }
        public int? OrderCount { get; set; }
        public double? OrderWeight { get; set; }
        public decimal? OrderCost { get; set; }
        public double? Delivered { get; set; }
        public DateTime? MarriageDate { get; set; }
    }
}