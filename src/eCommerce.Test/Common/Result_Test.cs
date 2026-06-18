using eCommerce.Application.Common;
using FluentAssertions;

namespace eCommerce.Test.Common;

/// <summary>
/// Unit tests for the <see cref="Result{T}"/> discriminated-result type and its <see cref="Error"/> payload.
/// Pure value logic: no dependencies, no mocks.
/// </summary>
public class Result_Test
{
    [Fact]
    public void Success_WhenGivenValue_ShouldExposeValueAndNoError()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Value.Should().Be(42);
        result.Error.Should().BeNull();
    }

    [Fact]
    public void BadRequest_WhenCalled_ShouldProduceValidationErrorWithDefaultCode()
    {
        var result = Result<string>.BadRequest("bad input");

        result.IsFailure.Should().BeTrue();
        result.Value.Should().BeNull();
        result.Error!.Type.Should().Be(ErrorType.Validation);
        result.Error.Code.Should().Be("bad_request");
        result.Error.Message.Should().Be("bad input");
    }

    [Fact]
    public void Validation_WhenGivenFieldErrors_ShouldCarryErrorList()
    {
        var errors = new[] { "name required", "email invalid" };

        var result = Result<string>.Validation("invalid", errors);

        result.Error!.Type.Should().Be(ErrorType.Validation);
        result.Error.Errors.Should().BeEquivalentTo(errors);
    }

    [Fact]
    public void NotFound_WhenCalled_ShouldProduceNotFoundError()
    {
        var result = Result<int>.NotFound("missing");

        result.Error!.Type.Should().Be(ErrorType.NotFound);
        result.Error.Code.Should().Be("not_found");
    }

    [Fact]
    public void Conflict_WhenCalled_ShouldProduceConflictError()
    {
        var result = Result<int>.Conflict("already exists");

        result.Error!.Type.Should().Be(ErrorType.Conflict);
        result.Error.Code.Should().Be("conflict");
    }

    [Fact]
    public void Failure_WhenCalledWithDefaults_ShouldProduceFailureError()
    {
        var result = Result<int>.Failure("boom");

        result.Error!.Type.Should().Be(ErrorType.Failure);
        result.Error.Code.Should().Be("failure");
    }

    [Fact]
    public void Error_WhenFieldsMatch_ShouldBeValueEqual()
    {
        // Error is a record → structural equality by all components.
        var a = new Error("c", "m", ErrorType.Failure);
        var b = new Error("c", "m", ErrorType.Failure);

        a.Should().Be(b);
    }
}
