using FluentValidation;
using TakweneTask.Application.DTOs.Tracks;

namespace TakweneTask.Application.Validators.Tracks;

public class UpdateTrackStatusRequestValidator : AbstractValidator<UpdateTrackStatusRequest>
{
    public UpdateTrackStatusRequestValidator()
    {
        RuleFor(x => x.Status)
            .IsInEnum()
            .WithMessage("Invalid track status. Valid values are: draft, submitted, distributed.");
    }
}
