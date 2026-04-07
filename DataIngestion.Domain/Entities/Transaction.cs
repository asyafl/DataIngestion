namespace DataIngestion.Domain.Entities
{
    public class Transaction
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = default!;
        public DateTime TransactionDateUtc { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public string SourceChannel { get; set; } = default!;
        public string DeduplicationKey { get; set; } = default!;
        public DateTime CreatedAtUtc { get; set; }
    }
}
