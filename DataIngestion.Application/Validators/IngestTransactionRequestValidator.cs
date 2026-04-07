using DataIngestion.Application.DTOs;
using FluentValidation;

namespace DataIngestion.Application.Validators
{
    public class IngestTransactionRequestValidator : AbstractValidator<IngestTransactionRequest>
    {
        public IngestTransactionRequestValidator()
        {
            RuleFor(x => x.CustomerId)
                .NotEmpty()
                .WithMessage("CustomerId is required.")
                .MaximumLength(100);


            RuleFor(x => x.TransactionDateUtc)
            .NotEmpty().WithMessage("TransactionDateUtc is required.")
            .Must(BeInPastOrPresent)
            .WithMessage("TransactionDateUtc cannot be in the future.");

            RuleFor(x => x.Amount)
            .GreaterThan(0).WithMessage("Amount must be greater than 0.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Length(3).WithMessage("Currency must be a 3-letter code.")
                .Must(BeUpperCase)
                .WithMessage("Currency must be uppercase (e.g., USD).");

            RuleFor(x => x.SourceChannel)
                .NotEmpty().WithMessage("SourceChannel is required.")
                .MaximumLength(50);
        }

        private static bool BeInPastOrPresent(DateTime date)
        {
            return date <= DateTime.UtcNow.AddMinutes(1);
        }

        private static bool BeUpperCase(string currency)
        {
            return currency == currency.ToUpperInvariant();
        }
    }
}
