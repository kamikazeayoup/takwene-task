using TakweneTask.Application.Common.Models;

namespace TakweneTask.Tests.Application;

public class ResultTests
{
    [Fact]
    public void Success_CreatesSuccessfulResultWithValue()
    {
        var result = Result<string>.Success("test");

        Assert.True(result.IsSuccess);
        Assert.Equal("test", result.Value);
        Assert.Equal(ErrorType.None, result.ErrorType);
        Assert.Null(result.ErrorMessage);
    }

    [Fact]
    public void NotFound_CreatesFailureResultWithNotFoundType()
    {
        var result = Result<string>.NotFound("Not found entity");

        Assert.False(result.IsSuccess);
        Assert.Null(result.Value);
        Assert.Equal(ErrorType.NotFound, result.ErrorType);
        Assert.Equal("Not found entity", result.ErrorMessage);
    }

    [Fact]
    public void Conflict_CreatesFailureResultWithConflictType()
    {
        var result = Result<string>.Conflict("Rule violation");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Conflict, result.ErrorType);
        Assert.Equal("Rule violation", result.ErrorMessage);
    }

    [Fact]
    public void Unauthorized_CreatesFailureResultWithUnauthorizedType()
    {
        var result = Result<string>.Unauthorized("Invalid credentials");

        Assert.False(result.IsSuccess);
        Assert.Equal(ErrorType.Unauthorized, result.ErrorType);
        Assert.Equal("Invalid credentials", result.ErrorMessage);
    }
}
