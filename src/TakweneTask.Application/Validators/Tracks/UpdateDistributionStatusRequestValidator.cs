using FluentValidation;
using TakweneTask.Application.DTOs.Tracks;
using TakweneTask.Domain.Enums;

namespace TakweneTask.Application.Validators.Tracks;

public class UpdateDistributionStatusRequestValidator : AbstractValidator<UpdateDistributionStatusRequest>
{
    public UpdateDistributionStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Status must be a valid distribution status.")
            .Must(s => s == DistributionStatus.Live || s == DistributionStatus.Rejected)
            .WithMessage("Status must be either 'live' or 'rejected'.");
    }
}
