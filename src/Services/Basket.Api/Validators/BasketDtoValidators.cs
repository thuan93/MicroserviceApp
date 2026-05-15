using Basket.Api.DTOs;
using FluentValidation;

namespace Basket.Api.Validators;

public class AddItemDtoValidator : AbstractValidator<AddItemDto>
{
    public AddItemDtoValidator()
    {
        RuleFor(x => x.ProductId)
            .GreaterThan(0);

        RuleFor(x => x.ProductName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(x => x.Quantity)
            .GreaterThan(0);

        RuleFor(x => x.Price)
            .GreaterThanOrEqualTo(0);
    }
}

public class UpdateItemQuantityDtoValidator : AbstractValidator<UpdateItemQuantityDto>
{
    public UpdateItemQuantityDtoValidator()
    {
        RuleFor(x => x.Quantity)
            .GreaterThanOrEqualTo(0);
    }
}

public class CheckoutDtoValidator : AbstractValidator<CheckoutDto>
{
    public CheckoutDtoValidator()
    {
        RuleFor(x => x.UserName)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.ShippingAddress)
            .MaximumLength(500);

        RuleFor(x => x.ShippingCity)
            .MaximumLength(100);

        RuleFor(x => x.ShippingCountry)
            .MaximumLength(100);
    }
}
