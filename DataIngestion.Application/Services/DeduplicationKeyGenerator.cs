using DataIngestion.Application.Abstractions;
using DataIngestion.Application.Exceptions;
using System.Globalization;

namespace DataIngestion.Application.Services
{
    public class DeduplicationKeyGenerator : IDeduplicationKeyGenerator
    {
        public string Generate(string customerId, DateTime transactionDateUtc, decimal amount, string currency, string sourceChannel)
        {
            if (string.IsNullOrWhiteSpace(customerId))
                throw new DomainValidationException("CustomerId is required");

            if (string.IsNullOrWhiteSpace(currency))
                throw new DomainValidationException("Currency is required");

            if (string.IsNullOrWhiteSpace(sourceChannel))
                throw new DomainValidationException("SourceChannel is required");


            var normalizedCustomerId = customerId.Trim();
            var normalizedCurrency = currency.Trim().ToUpperInvariant();
            var normalizedSourceChannel = sourceChannel.Trim().ToLowerInvariant();
            var normalizedTransactionDateUtc = transactionDateUtc.ToUniversalTime().ToString("O", CultureInfo.InvariantCulture);
            var normalizedAmount = amount.ToString("0.00", CultureInfo.InvariantCulture);

            return string.Join('|',
                normalizedCustomerId,
                normalizedTransactionDateUtc,
                normalizedAmount,
                normalizedCurrency,
                normalizedSourceChannel);
        }
    }
}
