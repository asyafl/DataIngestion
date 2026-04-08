namespace DataIngestion.Application.DTOs
{
    public class StatsBySourceChannelResponse
    {
        public string SourceChannel { get; set; } = default!;
        public int Count { get; set; }
    }
}
