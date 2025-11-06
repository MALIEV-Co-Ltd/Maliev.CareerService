using FluentValidation.TestHelper;
using Maliev.CareerService.Api.Models.DevelopmentGoals;
using Maliev.CareerService.Api.Validators;
using Maliev.CareerService.Data.Models;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for UpdateDevelopmentGoalRequestValidator
/// </summary>
public class UpdateDevelopmentGoalRequestValidatorTests
{
    private readonly UpdateDevelopmentGoalRequestValidator _validator;

    public UpdateDevelopmentGoalRequestValidatorTests()
    {
        _validator = new UpdateDevelopmentGoalRequestValidator();
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
    public void Validate_GoalTitleEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.GoalTitle = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GoalTitle);
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
        result.ShouldHaveValidationErrorFor(x => x.GoalTitle);
    }

    [Fact]
    public void Validate_GoalDescriptionEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.GoalDescription = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.GoalDescription);
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
        result.ShouldHaveValidationErrorFor(x => x.GoalDescription);
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

    [Fact]
    public void Validate_CategoryInvalid_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Category = "Invalid";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category);
    }

    [Fact]
    public void Validate_TargetDateTwoDaysAgo_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.TargetDate = DateTime.UtcNow.AddDays(-2);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.TargetDate);
    }

    [Fact]
    public void Validate_TargetDateToday_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.TargetDate = DateTime.UtcNow;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.TargetDate);
    }

    [Fact]
    public void Validate_ActionItemsTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ActionItems = new string('A', 4001);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ActionItems);
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
        result.ShouldHaveValidationErrorFor(x => x.ProgressNotes);
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
        result.ShouldHaveValidationErrorFor(x => x.RowVersion);
    }

    private UpdateDevelopmentGoalRequest CreateValidRequest()
    {
        return new UpdateDevelopmentGoalRequest
        {
            GoalTitle = "Master C# Advanced Patterns",
            GoalDescription = "Learn and apply advanced C# patterns",
            Category = DevelopmentGoalCategory.Technical,
            TargetDate = DateTime.UtcNow.AddMonths(6),
            ActionItems = "Complete course",
            ProgressNotes = "50% complete",
            RowVersion = Convert.ToBase64String(new byte[8])
        };
    }
}
