using TakweneTask.Application.DTOs.Artists;
using TakweneTask.Application.Validators.Artists;

namespace TakweneTask.Tests.Application;

public class CreateArtistRequestValidatorTests
{
    private readonly CreateArtistRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_PassesValidation()
    {
        var request = new CreateArtistRequest
        {
            Name = "Amr Diab",
            Email = "amr@example.com",
            Country = "EG"
        };

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void EmptyName_FailsValidation(string? name)
    {
        var request = new CreateArtistRequest
        {
            Name = name!,
            Email = "amr@example.com",
            Country = "EG"
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateArtistRequest.Name));
    }

    [Fact]
    public void NameExceeding200Chars_FailsValidation()
    {
        var request = new CreateArtistRequest
        {
            Name = new string('A', 201),
            Email = "amr@example.com",
            Country = "EG"
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateArtistRequest.Name));
    }

    [Theory]
    [InlineData("plainaddress")]
    [InlineData("user@")]
    [InlineData("@domain.com")]
    [InlineData("missingdomain@")]
    public void InvalidEmail_FailsValidation(string email)
    {
        var request = new CreateArtistRequest
        {
            Name = "Amr Diab",
            Email = email,
            Country = "EG"
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateArtistRequest.Email));
    }

    [Theory]
    [InlineData("E")]
    [InlineData("EGY")]
    [InlineData("eg")]
    [InlineData("12")]
    public void InvalidCountryCode_FailsValidation(string country)
    {
        var request = new CreateArtistRequest
        {
            Name = "Amr Diab",
            Email = "amr@example.com",
            Country = country
        };

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == nameof(CreateArtistRequest.Country));
    }
}
