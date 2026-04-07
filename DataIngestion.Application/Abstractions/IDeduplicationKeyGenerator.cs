namespace DataIngestion.Application.Abstractions
{
    public interface IDeduplicationKeyGenerator
    {
        public string Generate(string customerId, DateTime transactionDateUtc, decimal amount, string currency, string sourceChannel);
    }
}
