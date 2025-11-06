using FluentValidation.TestHelper;
using Maliev.CareerService.Api.Models.Applications;
using Maliev.CareerService.Api.Validators;
using Maliev.CareerService.Data.Models;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for UpdateApplicationStatusRequestValidator
/// </summary>
public class UpdateApplicationStatusRequestValidatorTests
{
    private readonly UpdateApplicationStatusRequestValidator _validator;

    public UpdateApplicationStatusRequestValidatorTests()
    {
        _validator = new UpdateApplicationStatusRequestValidator();
    }

    #region Valid Request Tests

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

    #endregion

    #region NewStatus Tests

    [Fact]
    public void Validate_NewStatusEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.NewStatus = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewStatus)
            .WithErrorMessage("New status is required");
    }

    [Fact]
    public void Validate_NewStatusTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.NewStatus = new string('A', 51);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewStatus)
            .WithErrorMessage("Status cannot exceed 50 characters");
    }

    [Theory]
    [InlineData(ApplicationStatus.Submitted)]
    [InlineData(ApplicationStatus.UnderReview)]
    [InlineData(ApplicationStatus.Interviewing)]
    [InlineData(ApplicationStatus.Offered)]
    [InlineData(ApplicationStatus.Accepted)]
    [InlineData(ApplicationStatus.Rejected)]
    [InlineData(ApplicationStatus.Withdrawn)]
    public void Validate_NewStatusValid_PassesValidation(string status)
    {
        // Arrange
        var request = CreateValidRequest();
        request.NewStatus = status;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NewStatus);
    }

    [Theory]
    [InlineData("submitted")]      // case insensitive - should pass
    [InlineData("UNDER_REVIEW")]   // case insensitive - should pass
    [InlineData("interviewing")]   // case insensitive - should pass
    public void Validate_NewStatusValidCaseInsensitive_PassesValidation(string status)
    {
        // Arrange
        var request = CreateValidRequest();
        request.NewStatus = status;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.NewStatus);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("Pending")]
    [InlineData("Completed")]
    public void Validate_NewStatusInvalid_FailsValidation(string status)
    {
        // Arrange
        var request = CreateValidRequest();
        request.NewStatus = status;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.NewStatus);
    }

    #endregion

    #region Reason Tests

    [Fact]
    public void Validate_ReasonTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Reason = new string('A', 1001);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Reason)
            .WithErrorMessage("Reason cannot exceed 1000 characters");
    }

    [Fact]
    public void Validate_ReasonNull_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Reason = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Validate_ReasonEmpty_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Reason = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    [Fact]
    public void Validate_ReasonAtMaxLength_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Reason = new string('A', 1000);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Reason);
    }

    #endregion

    #region RowVersion Tests

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
            .WithErrorMessage("Row version is required");
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
        result.ShouldHaveValidationErrorFor(x => x.RowVersion)
            .WithErrorMessage("Row version is required");
    }

    [Fact]
    public void Validate_RowVersionValid_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.RowVersion = Convert.ToBase64String(new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 });

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RowVersion);
    }

    #endregion

    #region Helper Methods

    private UpdateApplicationStatusRequest CreateValidRequest()
    {
        return new UpdateApplicationStatusRequest
        {
            NewStatus = ApplicationStatus.UnderReview,
            Reason = "Moving to next stage",
            RowVersion = Convert.ToBase64String(new byte[8]),
            IsReversal = false
        };
    }

    #endregion
}
