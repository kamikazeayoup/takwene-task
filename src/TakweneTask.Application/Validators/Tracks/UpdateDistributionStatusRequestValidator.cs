using FluentValidation;
using TakweneTask.Application.DTOs.Tracks;

namespace TakweneTask.Application.Validators.Tracks;

public class UpdateDistributionStatusRequestValidator : AbstractValidator<UpdateDistributionStatusRequest>
{
    public UpdateDistributionStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required.")
            .Must(s => s.Equals("live", StringComparison.OrdinalIgnoreCase) || s.Equals("rejected", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Status must be either 'live' or 'rejected'.");
    }
}
