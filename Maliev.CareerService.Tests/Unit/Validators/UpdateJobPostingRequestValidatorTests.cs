using FluentValidation.TestHelper;
using Maliev.CareerService.Api.Models.JobPostings;
using Maliev.CareerService.Api.Validators;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for UpdateJobPostingRequestValidator
/// </summary>
public class UpdateJobPostingRequestValidatorTests
{
    private readonly UpdateJobPostingRequestValidator _validator;

    public UpdateJobPostingRequestValidatorTests()
    {
        _validator = new UpdateJobPostingRequestValidator();
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

    #region PositionTitle Tests

    [Fact]
    public void Validate_PositionTitleEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.PositionTitle = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PositionTitle)
            .WithErrorMessage("Position title is required");
    }

    [Fact]
    public void Validate_PositionTitleTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.PositionTitle = new string('A', 201);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PositionTitle)
            .WithErrorMessage("Position title cannot exceed 200 characters");
    }

    #endregion

    #region EmploymentType Tests

    [Theory]
    [InlineData("Full-time")]
    [InlineData("Part-time")]
    [InlineData("Contract")]
    [InlineData("Internship")]
    [InlineData("full-time")]
    public void Validate_EmploymentTypeValid_PassesValidation(string employmentType)
    {
        // Arrange
        var request = CreateValidRequest();
        request.EmploymentType = employmentType;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.EmploymentType);
    }

    [Theory]
    [InlineData("Temporary")]
    [InlineData("Invalid")]
    public void Validate_EmploymentTypeInvalid_FailsValidation(string employmentType)
    {
        // Arrange
        var request = CreateValidRequest();
        request.EmploymentType = employmentType;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmploymentType);
    }

    #endregion

    #region Salary Tests

    [Fact]
    public void Validate_SalaryMinNegative_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.SalaryMin = -1;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SalaryMin);
    }

    [Fact]
    public void Validate_SalaryMinGreaterThanMax_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.SalaryMin = 100000;
        request.SalaryMax = 50000;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x);
    }

    #endregion

    #region Currency Tests

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("THB")]
    public void Validate_CurrencyValid_PassesValidation(string currency)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = currency;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Currency);
    }

    [Theory]
    [InlineData("US")]
    [InlineData("USDD")]
    [InlineData("us1")]
    public void Validate_CurrencyInvalid_FailsValidation(string currency)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = currency;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    #endregion

    #region Description, Requirements, Responsibilities Tests

    [Fact]
    public void Validate_DescriptionEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Description = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_DescriptionTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Description = new string('A', 10001);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Validate_RequirementsEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Requirements = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Requirements);
    }

    [Fact]
    public void Validate_ResponsibilitiesEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Responsibilities = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Responsibilities);
    }

    #endregion

    #region ApplicationDeadline Tests

    [Fact]
    public void Validate_ApplicationDeadlineInPast_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicationDeadline = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicationDeadline);
    }

    [Fact]
    public void Validate_ApplicationDeadlineInFuture_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicationDeadline = DateTime.UtcNow.AddMonths(1);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.ApplicationDeadline);
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

    [Theory]
    [InlineData("not-base64!")]
    [InlineData("invalid@string")]
    [InlineData("12345")]
    public void Validate_RowVersionInvalidBase64_FailsValidation(string rowVersion)
    {
        // Arrange
        var request = CreateValidRequest();
        request.RowVersion = rowVersion;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.RowVersion)
            .WithErrorMessage("RowVersion must be a valid Base64 string");
    }

    [Theory]
    [InlineData("AAAAAAAAAAA=")]
    [InlineData("dGVzdA==")]
    [InlineData("MTIzNDU2Nzg=")]
    public void Validate_RowVersionValidBase64_PassesValidation(string rowVersion)
    {
        // Arrange
        var request = CreateValidRequest();
        request.RowVersion = rowVersion;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.RowVersion);
    }

    #endregion

    #region Helper Methods

    private UpdateJobPostingRequest CreateValidRequest()
    {
        return new UpdateJobPostingRequest
        {
            PositionTitle = "Software Engineer",
            Department = "Engineering",
            Location = "Bangkok",
            EmploymentType = "Full-time",
            SalaryMin = 50000,
            SalaryMax = 100000,
            Currency = "THB",
            Description = "Job description",
            Requirements = "Job requirements",
            Responsibilities = "Job responsibilities",
            ApplicationDeadline = DateTime.UtcNow.AddMonths(1),
            RowVersion = Convert.ToBase64String(new byte[8])
        };
    }

    #endregion
}
