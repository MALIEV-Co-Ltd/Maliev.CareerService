using FluentValidation.TestHelper;
using Maliev.CareerService.Api.Models.Enrollments;
using Maliev.CareerService.Api.Validators;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for EnrollInTrainingRequestValidator
/// </summary>
public class EnrollInTrainingRequestValidatorTests
{
    private readonly EnrollInTrainingRequestValidator _validator;

    public EnrollInTrainingRequestValidatorTests()
    {
        _validator = new EnrollInTrainingRequestValidator();
    }

    [Fact]
    public void Validate_WithValidRequest_PassesValidation()
    {
        // Arrange
        var request = new EnrollInTrainingRequest
        {
            TrainingProgramId = Guid.NewGuid()
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validate_TrainingProgramIdEmpty_FailsValidation()
    {
        // Arrange
        var request = new EnrollInTrainingRequest
        {
            TrainingProgramId = Guid.Empty
        };

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TrainingProgramId);
    }
}
