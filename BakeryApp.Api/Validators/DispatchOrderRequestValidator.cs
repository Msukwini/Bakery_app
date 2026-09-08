using BakeryApp.Api.Controllers;
using FluentValidation;

namespace BakeryApp.Api.Validators;

public class DispatchOrderRequestValidator : AbstractValidator<DispatchOrderRequest>
{
    public DispatchOrderRequestValidator()
    {
        RuleFor(x => x.ResellerEmployeeId)
            .NotEmpty()
            .WithMessage("Reseller employee ID is required.");

        RuleFor(x => x.ProductVariantId)
            .NotEmpty()
            .WithMessage("Product variant ID is required.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Dispatch quantity must be greater than zero.");
    }
}