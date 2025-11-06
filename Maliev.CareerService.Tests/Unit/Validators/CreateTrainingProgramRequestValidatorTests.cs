using FluentValidation.TestHelper;
using Maliev.CareerService.Api.Models.TrainingPrograms;
using Maliev.CareerService.Api.Validators;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for CreateTrainingProgramRequestValidator
/// </summary>
public class CreateTrainingProgramRequestValidatorTests
{
    private readonly CreateTrainingProgramRequestValidator _validator;

    public CreateTrainingProgramRequestValidatorTests()
    {
        _validator = new CreateTrainingProgramRequestValidator();
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

    #region ProgramCode Tests

    [Fact]
    public void Validate_ProgramCodeEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ProgramCode = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProgramCode)
            .WithErrorMessage("Program code is required");
    }

    [Fact]
    public void Validate_ProgramCodeTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ProgramCode = new string('A', 51);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProgramCode)
            .WithErrorMessage("Program code cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("trn-001")]        // lowercase
    [InlineData("TRN_001")]        // underscore
    [InlineData("TRN 001")]        // space
    [InlineData("TRN-001!")]       // special char
    public void Validate_ProgramCodeInvalidFormat_FailsValidation(string code)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ProgramCode = code;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ProgramCode)
            .WithErrorMessage("Program code must contain only uppercase letters, numbers, and hyphens");
    }

    [Theory]
    [InlineData("TRN-001")]
    [InlineData("LEADERSHIP-2024")]
    [InlineData("ABC123")]
    public void Validate_ProgramCodeValidFormat_PassesValidation(string code)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ProgramCode = code;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ProgramCode);
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

    #region Category Tests

    [Fact]
    public void Validate_CategoryTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Category = new string('A', 101);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Category)
            .WithErrorMessage("Category cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_CategoryNull_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Category = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Category);
    }

    #endregion

    #region DurationHours Tests

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(-10.5)]
    public void Validate_DurationHoursZeroOrNegative_FailsValidation(decimal hours)
    {
        // Arrange
        var request = CreateValidRequest();
        request.DurationHours = hours;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.DurationHours)
            .WithErrorMessage("Duration hours must be greater than 0");
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
        result.ShouldHaveValidationErrorFor(x => x.DurationHours)
            .WithErrorMessage("Duration hours cannot exceed 9999.99");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(10.5)]
    [InlineData(100)]
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

    #region Provider Tests

    [Fact]
    public void Validate_ProviderTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Provider = new string('A', 201);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Provider)
            .WithErrorMessage("Provider cannot exceed 200 characters");
    }

    [Fact]
    public void Validate_ProviderNull_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Provider = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Provider);
    }

    #endregion

    #region ExternalLmsUrl Tests

    [Fact]
    public void Validate_ExternalLmsUrlTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExternalLmsUrl = "https://example.com/" + new string('a', 500);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExternalLmsUrl)
            .WithErrorMessage("External LMS URL cannot exceed 500 characters");
    }

    [Theory]
    [InlineData("not-a-url")]
    [InlineData("ftp://example.com")]
    [InlineData("example.com")]
    [InlineData("www.example.com")]
    public void Validate_ExternalLmsUrlInvalidFormat_FailsValidation(string url)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExternalLmsUrl = url;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ExternalLmsUrl)
            .WithErrorMessage("External LMS URL must be a valid URL");
    }

    [Theory]
    [InlineData("https://example.com")]
    [InlineData("http://example.com/training")]
    [InlineData("https://lms.company.com/course/123")]
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

    [Fact]
    public void Validate_ExternalLmsUrlNull_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ExternalLmsUrl = null;

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
    public void Validate_MaxParticipantsZeroOrNegative_FailsValidation(int maxParticipants)
    {
        // Arrange
        var request = CreateValidRequest();
        request.MaxParticipants = maxParticipants;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.MaxParticipants)
            .WithErrorMessage("Max participants must be greater than 0");
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
        result.ShouldHaveValidationErrorFor(x => x.MaxParticipants)
            .WithErrorMessage("Max participants cannot exceed 10000");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(50)]
    [InlineData(10000)]
    public void Validate_MaxParticipantsValid_PassesValidation(int maxParticipants)
    {
        // Arrange
        var request = CreateValidRequest();
        request.MaxParticipants = maxParticipants;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaxParticipants);
    }

    [Fact]
    public void Validate_MaxParticipantsNull_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.MaxParticipants = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.MaxParticipants);
    }

    #endregion

    #region Helper Methods

    private CreateTrainingProgramRequest CreateValidRequest()
    {
        return new CreateTrainingProgramRequest
        {
            ProgramCode = "TRN-001",
            ProgramName = "Leadership Training",
            Description = "Comprehensive leadership training program",
            Category = "Management",
            DurationHours = 40,
            Provider = "Training Provider Inc",
            ExternalLmsUrl = "https://lms.example.com/course/123",
            MaxParticipants = 30
        };
    }

    #endregion
}
