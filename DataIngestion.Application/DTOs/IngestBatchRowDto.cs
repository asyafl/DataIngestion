namespace DataIngestion.Application.DTOs
{
    public class IngestBatchRowDto
    {
        public int RowNumber { get; set; }
        public string CustomerId { get; set; } = default!;
        public DateTime TransactionDateUtc { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = default!;
        public string SourceChannel { get; set; } = default!;
    }
}
