namespace DataIngestion.Application.DTOs
{
    public class GetSummaryStatsResponse
    {
        public int TotalTransactions { get; set; }
        public decimal TotalAmount { get; set; }
        public int UniqueCustomers { get; set; }
        public List<StatsByCurrencyResponse> ByCurrency { get; set; } = [];
        public List<StatsBySourceChannelResponse> BySourceChannel { get; set; } = [];
    }
}
