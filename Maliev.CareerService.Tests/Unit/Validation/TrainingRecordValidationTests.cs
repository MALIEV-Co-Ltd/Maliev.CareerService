using Maliev.CareerService.Data.Enums;
using Maliev.CareerService.Data.Models;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validation;

/// <summary>
/// Unit tests for TrainingRecord business rule validation (Feature 003)
/// Tests validation logic that will be enforced at the service layer
/// </summary>
public class TrainingRecordValidationTests
{
    [Fact]
    public void CompletionDate_InThePast_IsValid()
    {
        // Arrange
        var completionDate = DateTime.UtcNow.AddDays(-10);

        // Act
        var isValid = IsCompletionDateValid(completionDate);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void CompletionDate_Today_IsValid()
    {
        // Arrange
        var completionDate = DateTime.UtcNow;

        // Act
        var isValid = IsCompletionDateValid(completionDate);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void CompletionDate_InTheFuture_IsInvalid()
    {
        // Arrange
        var completionDate = DateTime.UtcNow.AddDays(1);

        // Act
        var isValid = IsCompletionDateValid(completionDate);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ExpirationDate_AfterCompletionDate_IsValid()
    {
        // Arrange
        var completionDate = DateTime.UtcNow.AddDays(-10);
        var expirationDate = DateTime.UtcNow.AddDays(365);

        // Act
        var isValid = IsExpirationDateValid(completionDate, expirationDate);

        // Assert
        Assert.True(isValid);
    }

    [Fact]
    public void ExpirationDate_BeforeCompletionDate_IsInvalid()
    {
        // Arrange
        var completionDate = DateTime.UtcNow.AddDays(-10);
        var expirationDate = DateTime.UtcNow.AddDays(-20);

        // Act
        var isValid = IsExpirationDateValid(completionDate, expirationDate);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ExpirationDate_SameAsCompletionDate_IsInvalid()
    {
        // Arrange
        var date = DateTime.UtcNow.AddDays(-10);
        var completionDate = date;
        var expirationDate = date;

        // Act
        var isValid = IsExpirationDateValid(completionDate, expirationDate);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void ExpirationDate_Null_IsValid()
    {
        // Arrange
        var completionDate = DateTime.UtcNow.AddDays(-10);
        DateTime? expirationDate = null;

        // Act
        var isValid = IsExpirationDateValid(completionDate, expirationDate);

        // Assert
        Assert.True(isValid);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(50)]
    [InlineData(100)]
    public void Score_WithinRange_IsValid(decimal score)
    {
        // Act
        var isValid = IsScoreValid(score);

        // Assert
        Assert.True(isValid);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(-0.01)]
    [InlineData(100.01)]
    [InlineData(101)]
    [InlineData(200)]
    public void Score_OutsideRange_IsInvalid(decimal score)
    {
        // Act
        var isValid = IsScoreValid(score);

        // Assert
        Assert.False(isValid);
    }

    [Fact]
    public void Score_Null_IsValid()
    {
        // Arrange
        decimal? score = null;

        // Act
        var isValid = IsScoreValid(score);

        // Assert
        Assert.True(isValid);
    }

    /// <summary>
    /// Business rule: Completion date cannot be in the future
    /// </summary>
    private static bool IsCompletionDateValid(DateTime completionDate)
    {
        return completionDate <= DateTime.UtcNow;
    }

    /// <summary>
    /// Business rule: Expiration date must be after completion date (if provided)
    /// </summary>
    private static bool IsExpirationDateValid(DateTime completionDate, DateTime? expirationDate)
    {
        if (expirationDate == null)
            return true;

        return expirationDate.Value > completionDate;
    }

    /// <summary>
    /// Business rule: Score must be between 0 and 100 (if provided)
    /// </summary>
    private static bool IsScoreValid(decimal? score)
    {
        if (score == null)
            return true;

        return score.Value >= 0 && score.Value <= 100;
    }
}
