using FluentValidation.TestHelper;
using Maliev.CareerService.Api.Models.DevelopmentPlans;
using Maliev.CareerService.Api.Validators;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for UpdateIDPRequestValidator
/// </summary>
public class UpdateIDPRequestValidatorTests
{
    private readonly UpdateIDPRequestValidator _validator;

    public UpdateIDPRequestValidatorTests()
    {
        _validator = new UpdateIDPRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRowVersion_PassesValidation()
    {
        // Arrange
        var request = new UpdateIDPRequest
        {
            RowVersion = Convert.ToBase64String(new byte[8])
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
        var request = new UpdateIDPRequest
        {
            RowVersion = ""
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
        var request = new UpdateIDPRequest
        {
            RowVersion = null!
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RowVersion);
    }
}
