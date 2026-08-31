using FluentValidation;
using Ticketing.Api.Contracts.Events;

namespace Ticketing.Api.Validation;

public sealed class CreateEventRequestValidator : AbstractValidator<CreateEventRequest>
{
    public CreateEventRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
        RuleFor(x => x.Venue).NotEmpty().MaximumLength(200);
        RuleFor(x => x.TotalTicketCapacity).GreaterThan(0);
        RuleFor(x => x.PricingTiers).NotEmpty();
        RuleForEach(x => x.PricingTiers).SetValidator(new PricingTierRequestValidator());
        RuleFor(x => x).Must(HasTierCapacityWithinLimit)
            .WithMessage("Combined pricing tier capacity cannot exceed total event capacity.");
    }

    private static bool HasTierCapacityWithinLimit(CreateEventRequest request)
    {
        return request.PricingTiers.Sum(t => t.Capacity) <= request.TotalTicketCapacity;
    }
}
