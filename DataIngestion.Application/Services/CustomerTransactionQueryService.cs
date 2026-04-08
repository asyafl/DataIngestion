using DataIngestion.Application.Abstractions;
using DataIngestion.Application.DTOs;
using FluentValidation;
using Serilog;

namespace DataIngestion.Application.Services
{
    public class CustomerTransactionQueryService : ICustomerTransactionQueryService
    {
        private readonly ITransactionRepository _transactionRepository;
        private readonly IValidator<GetCustomerTransactionsRequest> _validator;
        private readonly ILogger _logger;

        public CustomerTransactionQueryService(ITransactionRepository transactionRepository, IValidator<GetCustomerTransactionsRequest> validator, ILogger logger)
        {
            _transactionRepository = transactionRepository;
            _validator = validator;
            _logger = logger;
        }

        public async Task<PagedResult<CustomerTransactionItemResponse>> GetPagedAsync(string customerId, GetCustomerTransactionsRequest query, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrEmpty(customerId))
            {
                _logger.Error("Customer ID is null or empty.");
                throw new ArgumentException("Customer ID cannot be null or empty.", nameof(customerId));
            }

            var validationResult = await _validator.ValidateAsync(query, cancellationToken);
            if (!validationResult.IsValid)
            {
                _logger.Error("Validation failed for GetCustomerTransactionsRequest: {Errors}", validationResult.Errors);
                throw new ValidationException(validationResult.Errors);
            }
            return await _transactionRepository.GetByCustomerAsync(customerId, query, cancellationToken);
        }
    }
}
