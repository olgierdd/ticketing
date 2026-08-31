using FluentValidation;
using Ticketing.Api.Contracts.Events;

namespace Ticketing.Api.Validation;

public sealed class PricingTierRequestValidator : AbstractValidator<PricingTierRequest>
{
    public PricingTierRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.Capacity).GreaterThan(0);
    }
}
