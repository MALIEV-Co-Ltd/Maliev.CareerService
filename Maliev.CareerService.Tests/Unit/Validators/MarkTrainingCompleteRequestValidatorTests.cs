using FluentValidation.TestHelper;
using Maliev.CareerService.Api.Models.Enrollments;
using Maliev.CareerService.Api.Validators;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for MarkTrainingCompleteRequestValidator
/// </summary>
public class MarkTrainingCompleteRequestValidatorTests
{
    private readonly MarkTrainingCompleteRequestValidator _validator;

    public MarkTrainingCompleteRequestValidatorTests()
    {
        _validator = new MarkTrainingCompleteRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_PassesValidation()
    {
        // Arrange
        var request = new MarkTrainingCompleteRequest
        {
            CompletionNotes = "Completed successfully",
            RowVersion = Convert.ToBase64String(new byte[8])
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_CompletionNotesTooLong_FailsValidation()
    {
        // Arrange
        var request = new MarkTrainingCompleteRequest
        {
            CompletionNotes = new string('A', 2001),
            RowVersion = Convert.ToBase64String(new byte[8])
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CompletionNotes)
            .WithErrorMessage("Completion notes cannot exceed 2000 characters");
    }

    [Fact]
    public void Validate_CompletionNotesNull_PassesValidation()
    {
        // Arrange
        var request = new MarkTrainingCompleteRequest
        {
            CompletionNotes = null,
            RowVersion = Convert.ToBase64String(new byte[8])
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CompletionNotes);
    }

    [Fact]
    public void Validate_RowVersionEmpty_FailsValidation()
    {
        // Arrange
        var request = new MarkTrainingCompleteRequest
        {
            CompletionNotes = "Completed",
            RowVersion = ""
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RowVersion)
            .WithErrorMessage("RowVersion is required for optimistic concurrency");
    }

    [Fact]
    public void Validate_RowVersionNull_FailsValidation()
    {
        // Arrange
        var request = new MarkTrainingCompleteRequest
        {
            CompletionNotes = "Completed",
            RowVersion = null!
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RowVersion);
    }
}
