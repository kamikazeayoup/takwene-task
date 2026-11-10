using FluentValidation;
using TakweneTask.Application.DTOs.Artists;

namespace TakweneTask.Application.Validators.Artists;

public class CreateArtistRequestValidator : AbstractValidator<CreateArtistRequest>
{
    public CreateArtistRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Artist name is required.")
            .MaximumLength(200)
            .WithMessage("Artist name must not exceed 200 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .MaximumLength(254)
            .WithMessage("Email must not exceed 254 characters.")
            .EmailAddress()
            .WithMessage("Invalid email address format.");

        RuleFor(x => x.Country)
            .NotEmpty()
            .WithMessage("Country is required.")
            .Length(2)
            .WithMessage("Country must be a 2-letter ISO 3166-1 alpha-2 code.")
            .Matches("^[A-Z]{2}$")
            .WithMessage("Country code must consist of two uppercase letters.");
    }
}
