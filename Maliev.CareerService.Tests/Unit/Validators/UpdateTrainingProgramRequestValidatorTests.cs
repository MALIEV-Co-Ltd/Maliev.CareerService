using FluentValidation.TestHelper;
using Maliev.CareerService.Api.Models.TrainingPrograms;
using Maliev.CareerService.Api.Validators;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for UpdateTrainingProgramRequestValidator
/// </summary>
public class UpdateTrainingProgramRequestValidatorTests
{
    private readonly UpdateTrainingProgramRequestValidator _validator;

    public UpdateTrainingProgramRequestValidatorTests()
    {
        _validator = new UpdateTrainingProgramRequestValidator();
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

    #region ProgramName Tests

    [Fact]
    public void Validate_ProgramNameEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ProgramName = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProgramName)
            .WithErrorMessage("Program name is required");
    }

    [Fact]
    public void Validate_ProgramNameTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ProgramName = new string('A', 201);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProgramName)
            .WithErrorMessage("Program name cannot exceed 200 characters");
    }

    #endregion

    #region Description Tests

    [Fact]
    public void Validate_DescriptionEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Description = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description is required");
    }

    #endregion

    #region DurationHours Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_DurationHoursZeroOrNegative_FailsValidation(decimal hours)
    {
        // Arrange
        var request = CreateValidRequest();
        request.DurationHours = hours;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationHours);
    }

    [Fact]
    public void Validate_DurationHoursTooLarge_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.DurationHours = 10000;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationHours);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(40.5)]
    [InlineData(9999.99)]
    public void Validate_DurationHoursValid_PassesValidation(decimal hours)
    {
        // Arrange
        var request = CreateValidRequest();
        request.DurationHours = hours;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.DurationHours);
    }

    #endregion

    #region ExternalLmsUrl Tests

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com")]
    public void Validate_ExternalLmsUrlInvalid_FailsValidation(string url)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExternalLmsUrl = url;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExternalLmsUrl);
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://lms.company.com")]
    public void Validate_ExternalLmsUrlValid_PassesValidation(string url)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExternalLmsUrl = url;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ExternalLmsUrl);
    }

    #endregion

    #region MaxParticipants Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Validate_MaxParticipantsZeroOrNegative_FailsValidation(int max)
    {
        // Arrange
        var request = CreateValidRequest();
        request.MaxParticipants = max;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxParticipants);
    }

    [Fact]
    public void Validate_MaxParticipantsTooLarge_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MaxParticipants = 10001;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxParticipants);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(10000)]
    public void Validate_MaxParticipantsValid_PassesValidation(int max)
    {
        // Arrange
        var request = CreateValidRequest();
        request.MaxParticipants = max;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaxParticipants);
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
            .WithErrorMessage("RowVersion is required for optimistic concurrency");
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
        result.ShouldHaveValidationErrorFor(x => x.RowVersion);
    }

    #endregion

    #region Helper Methods

    private UpdateTrainingProgramRequest CreateValidRequest()
    {
        return new UpdateTrainingProgramRequest
        {
            ProgramName = "Advanced Leadership Training",
            Description = "Updated comprehensive leadership training program",
            Category = "Management",
            DurationHours = 45,
            Provider = "Training Provider Inc",
            ExternalLmsUrl = "https://lms.example.com/course/123",
            MaxParticipants = 35,
            RowVersion = Convert.ToBase64String(new byte[8])
        };
    }

    #endregion
}
