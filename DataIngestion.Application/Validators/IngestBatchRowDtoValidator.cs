using DataIngestion.Application.DTOs;
using FluentValidation;

namespace DataIngestion.Application.Validators
{
    public class IngestBatchRowDtoValidator : AbstractValidator<IngestBatchRowDto>
    {
        public IngestBatchRowDtoValidator()
        {
            RuleFor(x => x.CustomerId)
            .NotEmpty().WithMessage("CustomerId is required.")
            .MaximumLength(100);

            RuleFor(x => x.TransactionDateUtc)
                .NotEmpty().WithMessage("TransactionDateUtc is required.")
                .Must(x => x <= DateTime.UtcNow.AddMinutes(1))
                .WithMessage("TransactionDateUtc cannot be in the future.");

            RuleFor(x => x.Amount)
                .GreaterThan(0).WithMessage("Amount must be greater than 0.");

            RuleFor(x => x.Currency)
                .NotEmpty().WithMessage("Currency is required.")
                .Length(3).WithMessage("Currency must be a 3-letter code.");

            RuleFor(x => x.SourceChannel)
                .NotEmpty().WithMessage("SourceChannel is required.")
                .MaximumLength(50);
        }
    }
}
