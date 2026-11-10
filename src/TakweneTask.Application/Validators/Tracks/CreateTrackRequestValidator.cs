using FluentValidation;
using TakweneTask.Application.Common.Interfaces;
using TakweneTask.Application.DTOs.Tracks;
using TakweneTask.Domain.Helpers;

namespace TakweneTask.Application.Validators.Tracks;

public class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequest>
{
    public CreateTrackRequestValidator(IUnitOfWork unitOfWork)
    {
        RuleFor(x => x.Title)
            .NotEmpty()
            .WithMessage("Track title is required.")
            .MaximumLength(200)
            .WithMessage("Track title must not exceed 200 characters.");

        RuleFor(x => x.ArtistId)
            .NotEmpty()
            .WithMessage("Artist ID is required.")
            .MustAsync(async (artistId, ct) => await unitOfWork.Artists.ExistsAsync(artistId, ct))
            .WithMessage("Artist does not exist.");

        RuleFor(x => x.Isrc)
            .NotEmpty()
            .WithMessage("ISRC is required.")
            .Must(isrc => Isrc.IsValid(isrc))
            .WithMessage("Invalid ISRC format. Expected 12 alphanumeric characters (e.g. USRC17607839).");

        RuleFor(x => x.ReleaseDate)
            .NotEmpty()
            .WithMessage("Release date is required.");

        RuleFor(x => x.Genre)
            .NotEmpty()
            .WithMessage("Genre is required.")
            .MaximumLength(50)
            .WithMessage("Genre must not exceed 50 characters.");
    }
}
