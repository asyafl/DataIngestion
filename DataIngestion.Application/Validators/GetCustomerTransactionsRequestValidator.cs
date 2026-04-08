using DataIngestion.Application.DTOs;
using FluentValidation;

namespace DataIngestion.Application.Validators
{
    public class GetCustomerTransactionsRequestValidator : AbstractValidator<GetCustomerTransactionsRequest>
    {
        public GetCustomerTransactionsRequestValidator()
        {
            RuleFor(x => x.Page)
                .GreaterThan(0)
                .WithMessage("Page must be greater than 0.");

            RuleFor(x => x.PageSize)
                .GreaterThan(0)
                .WithMessage("PageSize must be greater than 0.");

            RuleFor(x => x.FromDate)
                .LessThanOrEqualTo(x => x.ToDate)
                .When(x => x.FromDate.HasValue && x.ToDate.HasValue)
                .WithMessage("FromDate must be less than or equal to ToDate.");
        }
    }
}
