using FluentValidation.TestHelper;
using Maliev.CareerService.Api.Models.DevelopmentGoals;
using Maliev.CareerService.Api.Validators;
using Maliev.CareerService.Data.Models;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for CreateDevelopmentGoalRequestValidator
/// </summary>
public class CreateDevelopmentGoalRequestValidatorTests
{
    private readonly CreateDevelopmentGoalRequestValidator _validator;

    public CreateDevelopmentGoalRequestValidatorTests()
    {
        _validator = new CreateDevelopmentGoalRequestValidator();
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

    #region GoalTitle Tests

    [Fact]
    public void Validate_GoalTitleEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.GoalTitle = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GoalTitle)
            .WithErrorMessage("Goal title is required");
    }

    [Fact]
    public void Validate_GoalTitleTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.GoalTitle = new string('A', 201);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GoalTitle)
            .WithErrorMessage("Goal title cannot exceed 200 characters");
    }

    [Fact]
    public void Validate_GoalTitleAtMaxLength_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.GoalTitle = new string('A', 200);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GoalTitle);
    }

    #endregion

    #region GoalDescription Tests

    [Fact]
    public void Validate_GoalDescriptionEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.GoalDescription = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GoalDescription)
            .WithErrorMessage("Goal description is required");
    }

    [Fact]
    public void Validate_GoalDescriptionTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.GoalDescription = new string('A', 2001);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GoalDescription)
            .WithErrorMessage("Goal description cannot exceed 2000 characters");
    }

    [Fact]
    public void Validate_GoalDescriptionAtMaxLength_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.GoalDescription = new string('A', 2000);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.GoalDescription);
    }

    #endregion

    #region Category Tests

    [Fact]
    public void Validate_CategoryEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Category = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("Category is required");
    }

    [Theory]
    [InlineData(DevelopmentGoalCategory.Technical)]
    [InlineData(DevelopmentGoalCategory.Leadership)]
    [InlineData(DevelopmentGoalCategory.SoftSkills)]
    [InlineData(DevelopmentGoalCategory.Certification)]
    public void Validate_CategoryValid_PassesValidation(string category)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Category = category;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    [Theory]
    [InlineData("Invalid")]
    [InlineData("Management")]
    [InlineData("Business")]
    public void Validate_CategoryInvalid_FailsValidation(string category)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Category = category;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category);
    }

    #endregion

    #region TargetDate Tests

    [Fact]
    public void Validate_TargetDateInPast_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.TargetDate = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TargetDate)
            .WithErrorMessage("Target date must be in the future");
    }

    [Fact]
    public void Validate_TargetDateNow_MayPassOrFail()
    {
        // Arrange - DateTime.UtcNow is a timing-sensitive test
        // If test DateTime.UtcNow is evaluated before validator's DateTime.UtcNow,
        // it will fail (as expected). If after, it will pass (acceptable).
        // This is a known limitation of time-based validations in tests.
        var request = CreateValidRequest();
        request.TargetDate = DateTime.UtcNow.AddMilliseconds(-100); // Slightly in past to ensure failure

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TargetDate);
    }

    [Fact]
    public void Validate_TargetDateInFuture_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.TargetDate = DateTime.UtcNow.AddMonths(6);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TargetDate);
    }

    #endregion

    #region ActionItems Tests

    [Fact]
    public void Validate_ActionItemsTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ActionItems = new string('A', 4001);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ActionItems)
            .WithErrorMessage("Action items cannot exceed 4000 characters");
    }

    [Fact]
    public void Validate_ActionItemsAtMaxLength_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ActionItems = new string('A', 4000);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ActionItems);
    }

    [Fact]
    public void Validate_ActionItemsNull_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ActionItems = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ActionItems);
    }

    #endregion

    #region Helper Methods

    private CreateDevelopmentGoalRequest CreateValidRequest()
    {
        return new CreateDevelopmentGoalRequest
        {
            GoalTitle = "Master C# Advanced Patterns",
            GoalDescription = "Learn and apply advanced C# patterns in production code",
            Category = DevelopmentGoalCategory.Technical,
            TargetDate = DateTime.UtcNow.AddMonths(6),
            ActionItems = "1. Complete online course\n2. Practice in projects\n3. Code review sessions"
        };
    }

    #endregion
}
