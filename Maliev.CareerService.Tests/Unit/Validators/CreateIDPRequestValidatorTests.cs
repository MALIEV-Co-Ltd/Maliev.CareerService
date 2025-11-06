using FluentValidation.TestHelper;
using Maliev.CareerService.Api.Models.DevelopmentPlans;
using Maliev.CareerService.Api.Validators;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for CreateIDPRequestValidator
/// </summary>
public class CreateIDPRequestValidatorTests
{
    private readonly CreateIDPRequestValidator _validator;

    public CreateIDPRequestValidatorTests()
    {
        _validator = new CreateIDPRequestValidator();
    }

    [Fact]
    public void Validate_WithCurrentYear_PassesValidation()
    {
        // Arrange
        var request = new CreateIDPRequest
        {
            PlanYear = DateTime.UtcNow.Year
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithPreviousYear_PassesValidation()
    {
        // Arrange
        var request = new CreateIDPRequest
        {
            PlanYear = DateTime.UtcNow.Year - 1
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithNextYear_PassesValidation()
    {
        // Arrange
        var request = new CreateIDPRequest
        {
            PlanYear = DateTime.UtcNow.Year + 1
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithFiveYearsInFuture_PassesValidation()
    {
        // Arrange
        var request = new CreateIDPRequest
        {
            PlanYear = DateTime.UtcNow.Year + 5
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_WithTwoYearsAgo_FailsValidation()
    {
        // Arrange
        var request = new CreateIDPRequest
        {
            PlanYear = DateTime.UtcNow.Year - 2
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlanYear)
            .WithErrorMessage("Plan year must be current year or previous year (allow updates to previous year plans)");
    }

    [Fact]
    public void Validate_WithSixYearsInFuture_FailsValidation()
    {
        // Arrange
        var request = new CreateIDPRequest
        {
            PlanYear = DateTime.UtcNow.Year + 6
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlanYear)
            .WithErrorMessage("Plan year cannot be more than 5 years in the future");
    }

    [Fact]
    public void Validate_WithTenYearsInFuture_FailsValidation()
    {
        // Arrange
        var request = new CreateIDPRequest
        {
            PlanYear = DateTime.UtcNow.Year + 10
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PlanYear);
    }
}
