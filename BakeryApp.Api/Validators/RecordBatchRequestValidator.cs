using BakeryApp.Api.Controllers;
using FluentValidation;

namespace BakeryApp.Api.Validators;

public class RecordBatchRequestValidator : AbstractValidator<RecordBatchRequest>
{
    public RecordBatchRequestValidator()
    {
        RuleFor(x => x.ProductVariantId)
            .NotEmpty()
            .WithMessage("Product variant ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Batch quantity must be greater than zero.");
    }
}