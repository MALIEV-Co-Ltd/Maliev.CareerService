using FluentValidation.TestHelper;
using Maliev.CareerService.Api.Models.DevelopmentGoals;
using Maliev.CareerService.Api.Validators;
using Maliev.CareerService.Data.Models;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for UpdateGoalStatusRequestValidator
/// </summary>
public class UpdateGoalStatusRequestValidatorTests
{
    private readonly UpdateGoalStatusRequestValidator _validator;

    public UpdateGoalStatusRequestValidatorTests()
    {
        _validator = new UpdateGoalStatusRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_StatusEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Status = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status)
            .WithErrorMessage("Status is required");
    }

    [Theory]
    [InlineData(DevelopmentGoalStatus.NotStarted)]
    [InlineData(DevelopmentGoalStatus.InProgress)]
    [InlineData(DevelopmentGoalStatus.Completed)]
    [InlineData(DevelopmentGoalStatus.Deferred)]
    public void Validate_StatusValid_PassesValidation(string status)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Status = status;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Status);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("Pending")]
    [InlineData("Cancelled")]
    public void Validate_StatusInvalid_FailsValidation(string status)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Status = status;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Status);
    }

    [Fact]
    public void Validate_StatusCompletedWithoutCompletionDate_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Status = DevelopmentGoalStatus.Completed;
        request.CompletionDate = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompletionDate)
            .WithErrorMessage("Completion date is required when marking goal as Completed");
    }

    [Fact]
    public void Validate_StatusCompletedWithCompletionDate_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Status = DevelopmentGoalStatus.Completed;
        request.CompletionDate = DateTime.UtcNow;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CompletionDate);
    }

    [Fact]
    public void Validate_CompletionDateInFuture_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CompletionDate = DateTime.UtcNow.AddDays(2);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompletionDate)
            .WithErrorMessage("Completion date cannot be in the future");
    }

    [Fact]
    public void Validate_CompletionDateToday_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CompletionDate = DateTime.UtcNow;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CompletionDate);
    }

    [Fact]
    public void Validate_CompletionDateYesterday_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CompletionDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CompletionDate);
    }

    [Fact]
    public void Validate_ProgressNotesTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ProgressNotes = new string('A', 4001);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProgressNotes)
            .WithErrorMessage("Progress notes cannot exceed 4000 characters");
    }

    [Fact]
    public void Validate_ProgressNotesAtMaxLength_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ProgressNotes = new string('A', 4000);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProgressNotes);
    }

    [Fact]
    public void Validate_ProgressNotesNull_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ProgressNotes = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProgressNotes);
    }

    [Fact]
    public void Validate_RowVersionEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RowVersion = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RowVersion)
            .WithErrorMessage("Row version is required for optimistic concurrency control");
    }

    [Fact]
    public void Validate_RowVersionNull_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RowVersion = null!;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RowVersion);
    }

    private UpdateGoalStatusRequest CreateValidRequest()
    {
        return new UpdateGoalStatusRequest
        {
            Status = DevelopmentGoalStatus.InProgress,
            CompletionDate = null,
            ProgressNotes = "Making good progress",
            RowVersion = Convert.ToBase64String(new byte[8])
        };
    }
}
