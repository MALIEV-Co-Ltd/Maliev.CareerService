using FluentValidation.TestHelper;
using Maliev.CareerService.Api.Models.DevelopmentPlans;
using Maliev.CareerService.Api.Validators;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for ApproveIDPRequestValidator
/// </summary>
public class ApproveIDPRequestValidatorTests
{
    private readonly ApproveIDPRequestValidator _validator;

    public ApproveIDPRequestValidatorTests()
    {
        _validator = new ApproveIDPRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_PassesValidation()
    {
        // Arrange
        var request = new ApproveIDPRequest
        {
            RowVersion = Convert.ToBase64String(new byte[8]),
            ApprovalNotes = "Plan approved"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_RowVersionEmpty_FailsValidation()
    {
        // Arrange
        var request = new ApproveIDPRequest
        {
            RowVersion = "",
            ApprovalNotes = "Approved"
        };

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
        var request = new ApproveIDPRequest
        {
            RowVersion = null!,
            ApprovalNotes = "Approved"
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RowVersion);
    }

    [Fact]
    public void Validate_ApprovalNotesTooLong_FailsValidation()
    {
        // Arrange
        var request = new ApproveIDPRequest
        {
            RowVersion = Convert.ToBase64String(new byte[8]),
            ApprovalNotes = new string('A', 1001)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApprovalNotes)
            .WithErrorMessage("Approval notes cannot exceed 1000 characters");
    }

    [Fact]
    public void Validate_ApprovalNotesAtMaxLength_PassesValidation()
    {
        // Arrange
        var request = new ApproveIDPRequest
        {
            RowVersion = Convert.ToBase64String(new byte[8]),
            ApprovalNotes = new string('A', 1000)
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ApprovalNotes);
    }

    [Fact]
    public void Validate_ApprovalNotesNull_PassesValidation()
    {
        // Arrange
        var request = new ApproveIDPRequest
        {
            RowVersion = Convert.ToBase64String(new byte[8]),
            ApprovalNotes = null
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ApprovalNotes);
    }
}
