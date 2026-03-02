using Maliev.CareerService.Domain.Entities;
using System.ComponentModel.DataAnnotations;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Entities;

/// <summary>
/// Unit tests for TrainingRecord entity validation (Feature 003)
/// </summary>
public class TrainingRecordTests
{
    [Fact]
    public void TrainingRecord_ValidEntity_PassesValidation()
    {
        // Arrange
        var record = new TrainingRecord
        {
            EmployeeId = Guid.NewGuid(),
            CourseName = "Safety Training",
            CompletionDate = DateTime.UtcNow.AddDays(-1),
            TrainingType = TrainingType.InPerson,
            Status = TrainingStatus.Completed,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        var validationResults = ValidateEntity(record);

        // Assert
        Assert.Empty(validationResults);
    }

    [Fact]
    public void TrainingRecord_MissingEmployeeId_FailsValidation()
    {
        // Arrange
        var record = new TrainingRecord
        {
            EmployeeId = Guid.Empty,
            CourseName = "Safety Training",
            CompletionDate = DateTime.UtcNow.AddDays(-1),
            TrainingType = TrainingType.InPerson,
            Status = TrainingStatus.Completed,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        var validationResults = ValidateEntity(record);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(TrainingRecord.EmployeeId)));
    }

    [Fact]
    public void TrainingRecord_MissingCourseName_FailsValidation()
    {
        // Arrange
        var record = new TrainingRecord
        {
            EmployeeId = Guid.NewGuid(),
            CourseName = string.Empty,
            CompletionDate = DateTime.UtcNow.AddDays(-1),
            TrainingType = TrainingType.InPerson,
            Status = TrainingStatus.Completed,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        var validationResults = ValidateEntity(record);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(TrainingRecord.CourseName)));
    }

    [Fact]
    public void TrainingRecord_CourseNameTooLong_FailsValidation()
    {
        // Arrange
        var record = new TrainingRecord
        {
            EmployeeId = Guid.NewGuid(),
            CourseName = new string('A', 201), // Exceeds 200 char limit
            CompletionDate = DateTime.UtcNow.AddDays(-1),
            TrainingType = TrainingType.InPerson,
            Status = TrainingStatus.Completed,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        var validationResults = ValidateEntity(record);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(TrainingRecord.CourseName)));
    }

    [Fact]
    public void TrainingRecord_ScoreOutOfRange_FailsValidation()
    {
        // Arrange
        var record = new TrainingRecord
        {
            EmployeeId = Guid.NewGuid(),
            CourseName = "Safety Training",
            CompletionDate = DateTime.UtcNow.AddDays(-1),
            TrainingType = TrainingType.InPerson,
            Status = TrainingStatus.Completed,
            Score = 101, // Exceeds 100
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        var validationResults = ValidateEntity(record);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(TrainingRecord.Score)));
    }

    [Fact]
    public void TrainingRecord_NegativeScore_FailsValidation()
    {
        // Arrange
        var record = new TrainingRecord
        {
            EmployeeId = Guid.NewGuid(),
            CourseName = "Safety Training",
            CompletionDate = DateTime.UtcNow.AddDays(-1),
            TrainingType = TrainingType.InPerson,
            Status = TrainingStatus.Completed,
            Score = -1, // Below 0
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        var validationResults = ValidateEntity(record);

        // Assert
        Assert.NotEmpty(validationResults);
        Assert.Contains(validationResults, v => v.MemberNames.Contains(nameof(TrainingRecord.Score)));
    }

    [Fact]
    public void TrainingRecord_ValidScoreRange_PassesValidation()
    {
        // Arrange - Test boundary values
        var scores = new[] { 0m, 50m, 100m };

        foreach (var score in scores)
        {
            var record = new TrainingRecord
            {
                EmployeeId = Guid.NewGuid(),
                CourseName = "Safety Training",
                CompletionDate = DateTime.UtcNow.AddDays(-1),
                TrainingType = TrainingType.InPerson,
                Status = TrainingStatus.Completed,
                Score = score,
                CreatedBy = Guid.NewGuid(),
                UpdatedBy = Guid.NewGuid()
            };

            // Act
            var validationResults = ValidateEntity(record);

            // Assert
            Assert.Empty(validationResults);
        }
    }

    [Fact]
    public void TrainingRecord_OptionalFieldsNull_PassesValidation()
    {
        // Arrange
        var record = new TrainingRecord
        {
            EmployeeId = Guid.NewGuid(),
            CourseName = "Safety Training",
            CompletionDate = DateTime.UtcNow.AddDays(-1),
            TrainingType = TrainingType.InPerson,
            Status = TrainingStatus.Completed,
            TrainingProgramId = null,
            ExpirationDate = null,
            CertificateDocumentId = null,
            Provider = null,
            Score = null,
            CreatedBy = Guid.NewGuid(),
            UpdatedBy = Guid.NewGuid()
        };

        // Act
        var validationResults = ValidateEntity(record);

        // Assert
        Assert.Empty(validationResults);
    }

    /// <summary>
    /// Helper method to validate an entity using DataAnnotations
    /// </summary>
    private static List<ValidationResult> ValidateEntity(object entity)
    {
        var validationResults = new List<ValidationResult>();
        var validationContext = new ValidationContext(entity);
        Validator.TryValidateObject(entity, validationContext, validationResults, validateAllProperties: true);
        return validationResults;
    }
}
