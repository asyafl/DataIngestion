namespace DataIngestion.Application.DTOs
{
    public class IngestBatchResponse
    {
        public string FileName { get; set; } = default!;
        public int TotalRows { get; set; }
        public int AcceptedRows { get; set; }
        public int RejectedRows { get; set; }
        public List<BatchErrorResponse> Errors { get; set; } = [];
    }
}
