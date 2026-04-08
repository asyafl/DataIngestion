namespace DataIngestion.Application.DTOs
{
    public class StatsByCurrencyResponse
    {
        public string Currency { get; set; } = default!;
        public int Count { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
