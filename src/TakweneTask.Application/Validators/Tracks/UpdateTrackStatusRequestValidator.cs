using FluentValidation;
using TakweneTask.Application.DTOs.Tracks;
using TakweneTask.Domain.Enums;

namespace TakweneTask.Application.Validators.Tracks;

public class UpdateTrackStatusRequestValidator : AbstractValidator<UpdateTrackStatusRequest>
{
    public UpdateTrackStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .NotEmpty()
            .WithMessage("Status is required.")
            .Must(s => Enum.TryParse<TrackStatus>(s, true, out _))
            .WithMessage("Invalid track status. Valid values are: draft, submitted, distributed.");
    }
}
