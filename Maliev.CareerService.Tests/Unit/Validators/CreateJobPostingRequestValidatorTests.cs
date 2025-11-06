using FluentValidation.TestHelper;
using Maliev.CareerService.Api.Models.JobPostings;
using Maliev.CareerService.Api.Validators;
using Xunit;

namespace Maliev.CareerService.Tests.Unit.Validators;

/// <summary>
/// Unit tests for CreateJobPostingRequestValidator
/// </summary>
public class CreateJobPostingRequestValidatorTests
{
    private readonly CreateJobPostingRequestValidator _validator;

    public CreateJobPostingRequestValidatorTests()
    {
        _validator = new CreateJobPostingRequestValidator();
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

    #region PositionCode Tests

    [Fact]
    public void Validate_PositionCodeEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.PositionCode = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PositionCode)
            .WithErrorMessage("Position code is required");
    }

    [Fact]
    public void Validate_PositionCodeTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.PositionCode = new string('A', 51);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PositionCode)
            .WithErrorMessage("Position code cannot exceed 50 characters");
    }

    [Theory]
    [InlineData("se-001")]        // lowercase
    [InlineData("SE_001")]        // underscore
    [InlineData("SE 001")]        // space
    [InlineData("SE-001!")]       // special char
    public void Validate_PositionCodeInvalidFormat_FailsValidation(string invalidCode)
    {
        // Arrange
        var request = CreateValidRequest();
        request.PositionCode = invalidCode;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.PositionCode)
            .WithErrorMessage("Position code must contain only uppercase letters, numbers, and hyphens");
    }

    [Theory]
    [InlineData("SE-001")]
    [InlineData("SENIOR-ENG-2024")]
    [InlineData("A1B2C3")]
    public void Validate_PositionCodeValidFormat_PassesValidation(string validCode)
    {
        // Arrange
        var request = CreateValidRequest();
        request.PositionCode = validCode;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.PositionCode);
    }

    #endregion

    #region Department and Location Tests

    [Fact]
    public void Validate_DepartmentTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Department = new string('A', 101);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Department)
            .WithErrorMessage("Department cannot exceed 100 characters");
    }

    [Fact]
    public void Validate_LocationTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Location = new string('A', 101);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Location)
            .WithErrorMessage("Location cannot exceed 100 characters");
    }

    #endregion

    #region EmploymentType Tests

    [Fact]
    public void Validate_EmploymentTypeEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.EmploymentType = "";

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmploymentType)
            .WithErrorMessage("Employment type is required");
    }

    [Fact]
    public void Validate_EmploymentTypeTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.EmploymentType = new string('A', 51);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmploymentType);
    }

    [Theory]
    [InlineData("Full-time")]
    [InlineData("Part-time")]
    [InlineData("Contract")]
    [InlineData("Internship")]
    [InlineData("full-time")]  // case insensitive
    [InlineData("FULL-TIME")]  // case insensitive
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
    [InlineData("Freelance")]
    [InlineData("Invalid")]
    public void Validate_EmploymentTypeInvalid_FailsValidation(string employmentType)
    {
        // Arrange
        var request = CreateValidRequest();
        request.EmploymentType = employmentType;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.EmploymentType)
            .WithErrorMessage("Employment type must be one of: Full-time, Part-time, Contract, Internship");
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
        result.ShouldHaveValidationErrorFor(x => x.SalaryMin)
            .WithErrorMessage("Minimum salary must be non-negative");
    }

    [Fact]
    public void Validate_SalaryMinTooLarge_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.SalaryMin = 1000000000;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SalaryMin)
            .WithErrorMessage("Minimum salary cannot exceed 999999999.99");
    }

    [Fact]
    public void Validate_SalaryMaxNegative_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.SalaryMax = -1;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SalaryMax)
            .WithErrorMessage("Maximum salary must be non-negative");
    }

    [Fact]
    public void Validate_SalaryMaxTooLarge_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.SalaryMax = 1000000000;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.SalaryMax)
            .WithErrorMessage("Maximum salary cannot exceed 999999999.99");
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
        result.ShouldHaveValidationErrorFor(x => x)
            .WithErrorMessage("Minimum salary cannot exceed maximum salary");
    }

    [Fact]
    public void Validate_SalaryRangeValid_PassesValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.SalaryMin = 50000;
        request.SalaryMax = 100000;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.SalaryMin);
        result.ShouldNotHaveValidationErrorFor(x => x.SalaryMax);
        result.ShouldNotHaveValidationErrorFor(x => x);
    }

    #endregion

    #region Currency Tests

    [Theory]
    [InlineData("US")]         // too short
    [InlineData("USDD")]       // too long
    [InlineData("us1")]        // lowercase + number
    [InlineData("U$D")]        // special char
    public void Validate_CurrencyInvalidFormat_FailsValidation(string currency)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = currency;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Currency);
    }

    [Theory]
    [InlineData("USD")]
    [InlineData("EUR")]
    [InlineData("THB")]
    public void Validate_CurrencyValidFormat_PassesValidation(string currency)
    {
        // Arrange
        var request = CreateValidRequest();
        request.Currency = currency;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldNotHaveValidationErrorFor(x => x.Currency);
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
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description is required");
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
        result.ShouldHaveValidationErrorFor(x => x.Description)
            .WithErrorMessage("Description cannot exceed 10000 characters");
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
        result.ShouldHaveValidationErrorFor(x => x.Requirements)
            .WithErrorMessage("Requirements are required");
    }

    [Fact]
    public void Validate_RequirementsTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Requirements = new string('A', 10001);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Requirements)
            .WithErrorMessage("Requirements cannot exceed 10000 characters");
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
        result.ShouldHaveValidationErrorFor(x => x.Responsibilities)
            .WithErrorMessage("Responsibilities are required");
    }

    [Fact]
    public void Validate_ResponsibilitiesTooLong_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.Responsibilities = new string('A', 10001);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.Responsibilities)
            .WithErrorMessage("Responsibilities cannot exceed 10000 characters");
    }

    #endregion

    #region ApplicationDeadline Tests

    [Fact]
    public void Validate_ApplicationDeadlineEmpty_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicationDeadline = default;

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicationDeadline)
            .WithErrorMessage("Application deadline is required");
    }

    [Fact]
    public void Validate_ApplicationDeadlineInPast_FailsValidation()
    {
        // Arrange
        var request = CreateValidRequest();
        request.ApplicationDeadline = DateTime.UtcNow.AddDays(-1);

        // Act
        var result = _validator.TestValidate(request);

        // Assert
        result.ShouldHaveValidationErrorFor(x => x.ApplicationDeadline)
            .WithErrorMessage("Application deadline must be in the future");
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

    #region Helper Methods

    private CreateJobPostingRequest CreateValidRequest()
    {
        return new CreateJobPostingRequest
        {
            PositionTitle = "Software Engineer",
            PositionCode = "SE-001",
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
            PublishImmediately = true
        };
    }

    #endregion
}
