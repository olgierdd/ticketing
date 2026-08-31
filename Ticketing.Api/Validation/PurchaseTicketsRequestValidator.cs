using FluentValidation;
using Ticketing.Api.Contracts.Tickets;

namespace Ticketing.Api.Validation;

public sealed class PurchaseTicketsRequestValidator : AbstractValidator<PurchaseTicketsRequest>
{
    public PurchaseTicketsRequestValidator()
    {
        RuleFor(x => x.EventId).NotEmpty();
        RuleFor(x => x.PricingTierId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty().MaximumLength(200);
        RuleFor(x => x.CustomerEmail).NotEmpty().EmailAddress().MaximumLength(320);
        RuleFor(x => x.Quantity).GreaterThan(0).LessThanOrEqualTo(20);
    }
}
