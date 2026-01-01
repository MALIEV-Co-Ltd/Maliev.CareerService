using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Maliev.CareerService.Data.Enums;
using Maliev.CareerService.Data.Models;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Entities;

public class SkillTests
{
    [Fact]
    public void Skill_Validation_ShouldSucceed_WhenValid()
    {
        // Arrange
        var skill = new Skill
        {
            EmployeeId = Guid.NewGuid(),
            SkillName = "C#",
            ProficiencyLevel = ProficiencyLevel.Expert,
            LastAssessedDate = DateTime.UtcNow
        };

        // Act
        var validationContext = new ValidationContext(skill);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(skill, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeTrue();
    }

    [Theory]
    [InlineData("")]
    [InlineData(null)]
    public void Skill_Validation_ShouldFail_WhenSkillNameIsEmpty(string? name)
    {
        // Arrange
        var skill = new Skill
        {
            EmployeeId = Guid.NewGuid(),
            SkillName = name!,
            ProficiencyLevel = ProficiencyLevel.Advanced,
            LastAssessedDate = DateTime.UtcNow
        };

        // Act
        var validationContext = new ValidationContext(skill);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(skill, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().Contain(r => r.MemberNames.Contains("SkillName"));
    }

    [Fact]
    public void Skill_Validation_ShouldFail_WhenSkillNameTooLong()
    {
        // Arrange
        var skill = new Skill
        {
            EmployeeId = Guid.NewGuid(),
            SkillName = new string('A', 101),
            ProficiencyLevel = ProficiencyLevel.Advanced,
            LastAssessedDate = DateTime.UtcNow
        };

        // Act
        var validationContext = new ValidationContext(skill);
        var validationResults = new List<ValidationResult>();
        bool isValid = Validator.TryValidateObject(skill, validationContext, validationResults, true);

        // Assert
        isValid.Should().BeFalse();
        validationResults.Should().Contain(r => r.MemberNames.Contains("SkillName"));
    }
}
