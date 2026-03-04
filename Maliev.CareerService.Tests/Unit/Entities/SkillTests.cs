using System.ComponentModel.DataAnnotations;
using Maliev.CareerService.Domain.Entities;
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
        Assert.True(isValid);
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
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("SkillName"));
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
        Assert.False(isValid);
        Assert.Contains(validationResults, r => r.MemberNames.Contains("SkillName"));
    }
}
