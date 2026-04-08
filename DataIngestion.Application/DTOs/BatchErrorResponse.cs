namespace DataIngestion.Application.DTOs
{
    public class BatchErrorResponse
    {
        public string CustomerId { get; set; } = default!;
        public int RowNumber { get; set; }
        public string Error { get; set; } = default!;
    }
}
