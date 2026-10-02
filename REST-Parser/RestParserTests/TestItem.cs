using System;

namespace RestParserTests
{
    internal class TestItem
    {
        public int Id { get; set; }
        public string Surname { get; set; } = "";
        public string FirstName { get; set; } = "";
        public int Amount { get; set; }
        public double Price { get; set; }
        public decimal Rate { get; set; }
        public DateTime Birthday { get; set; }
        public bool Flag { get; set; }
        public bool? NullableFlag { get; set; }
        public string MiddleName { get; set; } = "";
        public int? OrderCount { get; set; }
        public double? OrderWeight { get; set; }
        public decimal? OrderCost { get; set; }
        public double? Delivered { get; set; }
        public DateTime? MarriageDate { get; set; }
        public Guid GuidId { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public DateTimeOffset? ShippedAt { get; set; }
        public long BigNumber { get; set; }
        public long? NullableBigNumber { get; set; }
        public float Weight { get; set; }
        public short Quantity { get; set; }
        public byte Level { get; set; }
        public TestStatus Status { get; set; }
        public TestStatus? NullableStatus { get; set; }
        public DateOnly StartDate { get; set; }
        public DateOnly? EndDate { get; set; }
        public TimeOnly OpensAt { get; set; }
        public TimeSpan Duration { get; set; }
        public TimeSpan? NullableDuration { get; set; }
    }

    internal enum TestStatus
    {
        Draft = 0,
        Active = 1,
        Archived = 2
    }
}