using FluentValidation.TestHelper;
using Maliev.CareerService.Api.Models.Applications;
using Maliev.CareerService.Api.Validators;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for SubmitJobApplicationRequestValidator
/// </summary>
public class SubmitJobApplicationRequestValidatorTests
{
    private readonly SubmitJobApplicationRequestValidator _validator;

    public SubmitJobApplicationRequestValidatorTests()
    {
        _validator = new SubmitJobApplicationRequestValidator();
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

    #region JobPostingId Tests

    [Fact]
    public void Validate_JobPostingIdEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.JobPostingId = Guid.Empty;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.JobPostingId)
            .WithErrorMessage("Job posting ID is required");
    }

    #endregion

    #region ApplicantFirstName Tests

    [Fact]
    public void Validate_ApplicantFirstNameEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicantFirstName = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicantFirstName)
            .WithErrorMessage("First name is required");
    }

    [Fact]
    public void Validate_ApplicantFirstNameTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicantFirstName = new string('A', 101);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicantFirstName)
            .WithErrorMessage("First name cannot exceed 100 characters");
    }

    #endregion

    #region ApplicantLastName Tests

    [Fact]
    public void Validate_ApplicantLastNameEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicantLastName = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicantLastName)
            .WithErrorMessage("Last name is required");
    }

    [Fact]
    public void Validate_ApplicantLastNameTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicantLastName = new string('A', 101);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicantLastName)
            .WithErrorMessage("Last name cannot exceed 100 characters");
    }

    #endregion

    #region ApplicantEmail Tests

    [Fact]
    public void Validate_ApplicantEmailEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicantEmail = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicantEmail)
            .WithErrorMessage("Email is required");
    }

    [Theory]
    [InlineData("not-an-email")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Validate_ApplicantEmailInvalidFormat_FailsValidation(string email)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicantEmail = email;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicantEmail)
            .WithErrorMessage("Invalid email format");
    }

    [Fact]
    public void Validate_ApplicantEmailTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicantEmail = new string('a', 247) + "@test.com"; // 256 chars (247 + 9)

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicantEmail)
            .WithErrorMessage("Email cannot exceed 255 characters");
    }

    [Theory]
    [InlineData("user@example.com")]
    [InlineData("first.last@company.co.th")]
    [InlineData("user+tag@domain.com")]
    public void Validate_ApplicantEmailValid_PassesValidation(string email)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicantEmail = email;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ApplicantEmail);
    }

    #endregion

    #region ApplicantPhone Tests

    [Fact]
    public void Validate_ApplicantPhoneTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicantPhone = new string('1', 21);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicantPhone)
            .WithErrorMessage("Phone cannot exceed 20 characters");
    }

    [Theory]
    [InlineData("abc123")]
    [InlineData("123-abc-456")]
    [InlineData("123@456")]
    public void Validate_ApplicantPhoneInvalidFormat_FailsValidation(string phone)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicantPhone = phone;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicantPhone)
            .WithErrorMessage("Invalid phone format");
    }

    [Theory]
    [InlineData("+66812345678")]
    [InlineData("0812345678")]
    [InlineData("+1 (555) 123-4567")]
    [InlineData("555-1234")]
    public void Validate_ApplicantPhoneValid_PassesValidation(string phone)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicantPhone = phone;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ApplicantPhone);
    }

    #endregion

    #region ApplicantCountryCode Tests

    [Theory]
    [InlineData("T")]          // too short
    [InlineData("THA")]        // too long
    [InlineData("th")]         // lowercase
    [InlineData("T1")]         // number
    public void Validate_ApplicantCountryCodeInvalidFormat_FailsValidation(string countryCode)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicantCountryCode = countryCode;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicantCountryCode);
    }

    [Theory]
    [InlineData("TH")]
    [InlineData("US")]
    [InlineData("GB")]
    public void Validate_ApplicantCountryCodeValid_PassesValidation(string countryCode)
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicantCountryCode = countryCode;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ApplicantCountryCode);
    }

    #endregion

    #region ResumeFileId Tests

    [Fact]
    public void Validate_ResumeFileIdEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ResumeFileId = Guid.Empty;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ResumeFileId)
            .WithErrorMessage("Resume file is required");
    }

    #endregion

    #region CoverLetter Tests

    [Fact]
    public void Validate_CoverLetterTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CoverLetter = new string('A', 5001);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.CoverLetter)
            .WithErrorMessage("Cover letter cannot exceed 5000 characters");
    }

    [Fact]
    public void Validate_CoverLetterNull_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.CoverLetter = null;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.CoverLetter);
    }

    #endregion

    #region AdditionalFileIds Tests

    [Fact]
    public void Validate_AdditionalFileIdsTooMany_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.AdditionalFileIds =
        [
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        ];

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AdditionalFileIds)
            .WithErrorMessage("Maximum 4 additional files allowed");
    }

    [Fact]
    public void Validate_AdditionalFileIdsDuplicates_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        var duplicateId = Guid.NewGuid();
        request.AdditionalFileIds = [duplicateId, duplicateId];

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.AdditionalFileIds)
            .WithErrorMessage("Duplicate file IDs are not allowed");
    }

    [Fact]
    public void Validate_AdditionalFilesAtMaxLimit_PassesValidation()
    {
        // Arrange - 4 additional + 1 resume = 5 total (at limit, valid)
        var request = CreateValidRequest();
        request.AdditionalFileIds =
        [
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid(),
            Guid.NewGuid()
        ];

        // Act
        var result = _validator.TestValidate(request);

        // Assert - Should pass (5 total files is allowed)
        result.ShouldNotHaveValidationErrorFor(x => x.AdditionalFileIds);
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    [Fact]
    public void Validate_AdditionalFilesEmpty_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.AdditionalFileIds = [];

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveAnyValidationErrors();
    }

    #endregion

    #region Helper Methods

    private SubmitJobApplicationRequest CreateValidRequest()
    {
        return new SubmitJobApplicationRequest
        {
            JobPostingId = Guid.NewGuid(),
            ApplicantFirstName = "John",
            ApplicantLastName = "Doe",
            ApplicantEmail = "john.doe@example.com",
            ApplicantPhone = "+66812345678",
            ApplicantCountryCode = "TH",
            ResumeFileId = Guid.NewGuid(),
            CoverLetter = "I am interested in this position.",
            AdditionalFileIds = [Guid.NewGuid(), Guid.NewGuid()]
        };
    }

    #endregion
}
