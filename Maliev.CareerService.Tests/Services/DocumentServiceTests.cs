using FluentAssertions;
using Maliev.CareerService.Api.Models;
using Maliev.CareerService.Api.Services;
using Maliev.CareerService.Data.DbContexts;
using Maliev.CareerService.Data.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Xunit;

namespace Maliev.CareerService.Tests.Services;

public class DocumentServiceTests : IDisposable
{
    private readonly Mock<IUploadServiceClient> _mockUploadServiceClient;
    private readonly Mock<ILogger<DocumentService>> _mockLogger;
    private readonly Mock<IOptions<GcsConfiguration>> _mockGcsConfiguration;
    private readonly CareerDbContext _context;
    private readonly DocumentService _documentService;

    public DocumentServiceTests()
    {
        var options = new DbContextOptionsBuilder<CareerDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new CareerDbContext(options);
        _mockUploadServiceClient = new Mock<IUploadServiceClient>();
        _mockLogger = new Mock<ILogger<DocumentService>>();
        _mockGcsConfiguration = new Mock<IOptions<GcsConfiguration>>();

        _mockGcsConfiguration.Setup(x => x.Value)
            .Returns(new GcsConfiguration
            {
                BasePath = "careers"
            });

        _documentService = new DocumentService(
            _context,
            _mockUploadServiceClient.Object,
            _mockLogger.Object,
            _mockGcsConfiguration.Object);

        SeedTestData();
    }

    private void SeedTestData()
    {
        var jobPosition = new JobPosition
        {
            Id = 1,
            Title = "Software Engineer",
            Department = "Engineering",
            Description = "Develop software applications",
            EmploymentType = "Full-time",
            ExperienceLevel = "Mid-level",
            IsActive = true,
            IsPublic = true
        };

        var jobApplication = new JobApplication
        {
            Id = 1,
            ApplicantName = "Test Applicant",
            ApplicantEmail = "test@example.com",
            JobPositionId = 1,
            Status = "Submitted"
        };

        _context.JobPositions.Add(jobPosition);
        _context.JobApplications.Add(jobApplication);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetByIdAsync_ExistingDocument_ReturnsDocumentDto()
    {
        // Arrange
        var document = new ApplicationDocument
        {
            Id = 1,
            JobApplicationId = 1,
            DocumentType = DocumentType.Resume,
            OriginalFileName = "resume.pdf",
            MimeType = "application/pdf",
            FileSize = 1024,
            GcsBucket = "test-bucket",
            GcsObjectName = "test-object",
            GcsUri = "gs://test-bucket/test-object",
            Description = "Test resume",
            IsRequired = true,
            DisplayOrder = 1
        };
        _context.ApplicationDocuments.Add(document);
        await _context.SaveChangesAsync();

        // Act
        var result = await _documentService.GetByIdAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(1);
        result.DocumentType.Should().Be(DocumentType.Resume);
        result.OriginalFileName.Should().Be("resume.pdf");
        result.MimeType.Should().Be("application/pdf");
        result.FileSize.Should().Be(1024);
        result.Description.Should().Be("Test resume");
        result.IsRequired.Should().BeTrue();
        result.DisplayOrder.Should().Be(1);
    }

    [Fact]
    public async Task GetByIdAsync_NonExistingDocument_ReturnsNull()
    {
        // Act
        var result = await _documentService.GetByIdAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task GetByApplicationIdAsync_ExistingDocuments_ReturnsOrderedList()
    {
        // Arrange
        var documents = new[]
        {
            new ApplicationDocument
            {
                Id = 1,
                JobApplicationId = 1,
                DocumentType = DocumentType.Resume,
                OriginalFileName = "resume.pdf",
                MimeType = "application/pdf",
                FileSize = 1024,
                DisplayOrder = 2,
                GcsBucket = "test-bucket",
                GcsObjectName = "resume",
                GcsUri = "gs://test-bucket/resume"
            },
            new ApplicationDocument
            {
                Id = 2,
                JobApplicationId = 1,
                DocumentType = DocumentType.CoverLetter,
                OriginalFileName = "cover.pdf",
                MimeType = "application/pdf",
                FileSize = 512,
                DisplayOrder = 1,
                GcsBucket = "test-bucket",
                GcsObjectName = "cover",
                GcsUri = "gs://test-bucket/cover"
            }
        };

        _context.ApplicationDocuments.AddRange(documents);
        await _context.SaveChangesAsync();

        // Act
        var result = await _documentService.GetByApplicationIdAsync(1);

        // Assert
        var documentList = result.ToList();
        documentList.Should().HaveCount(2);
        documentList[0].DisplayOrder.Should().Be(1); // Should be ordered by DisplayOrder
        documentList[1].DisplayOrder.Should().Be(2);
        documentList[0].DocumentType.Should().Be(DocumentType.CoverLetter);
        documentList[1].DocumentType.Should().Be(DocumentType.Resume);
    }

    [Fact]
    public async Task UploadDocumentAsync_ValidRequest_ReturnsDocumentDto()
    {
        // Arrange
        var formFile = CreateMockFormFile("test.pdf", "application/pdf", 1024);
        var request = new DocumentUploadRequest
        {
            File = formFile,
            DocumentType = DocumentType.Resume,
            Description = "Test resume",
            IsRequired = true,
            DisplayOrder = 1
        };

        var uploadResponse = new UploadServiceResponse
        {
            Bucket = "test-bucket",
            ObjectName = "test-object",
            Uri = "gs://test-bucket/test-object",
            Size = 1024
        };

        _mockUploadServiceClient
            .Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<UploadServiceRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(uploadResponse);

        // Act
        var result = await _documentService.UploadDocumentAsync(1, request);

        // Assert
        result.Should().NotBeNull();
        result.DocumentType.Should().Be(DocumentType.Resume);
        result.OriginalFileName.Should().Be("test.pdf");
        result.MimeType.Should().Be("application/pdf");
        result.FileSize.Should().Be(1024);
        result.GcsBucket.Should().Be("test-bucket");
        result.Description.Should().Be("Test resume");
        result.IsRequired.Should().BeTrue();
        result.DisplayOrder.Should().Be(1);

        // Verify database record was created
        var documentInDb = await _context.ApplicationDocuments.FirstAsync(d => d.Id == result.Id);
        documentInDb.Should().NotBeNull();
        documentInDb.JobApplicationId.Should().Be(1);
        documentInDb.OriginalFileName.Should().Be("test.pdf");
    }

    [Fact]
    public async Task UploadDocumentAsync_NonExistentApplication_ThrowsArgumentException()
    {
        // Arrange
        var formFile = CreateMockFormFile("test.pdf", "application/pdf", 1024);
        var request = new DocumentUploadRequest
        {
            File = formFile,
            DocumentType = DocumentType.Resume
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _documentService.UploadDocumentAsync(999, request));
    }

    [Theory]
    [InlineData("InvalidType")]
    [InlineData("")]
    public async Task UploadDocumentAsync_InvalidDocumentType_ThrowsArgumentException(string invalidType)
    {
        // Arrange
        var formFile = CreateMockFormFile("test.pdf", "application/pdf", 1024);
        var request = new DocumentUploadRequest
        {
            File = formFile,
            DocumentType = invalidType
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _documentService.UploadDocumentAsync(1, request));
    }

    [Theory]
    [InlineData("application/x-msdownload")]
    [InlineData("application/msword")]
    [InlineData("text/html")]
    [InlineData("image/gif")]
    public async Task UploadDocumentAsync_InvalidMimeType_ThrowsArgumentException(string invalidMimeType)
    {
        // Arrange
        var formFile = CreateMockFormFile("test.exe", invalidMimeType, 1024);
        var request = new DocumentUploadRequest
        {
            File = formFile,
            DocumentType = DocumentType.Resume
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _documentService.UploadDocumentAsync(1, request));
    }

    [Fact]
    public async Task UploadDocumentAsync_FileSizeExceedsLimit_ThrowsArgumentException()
    {
        // Arrange - File larger than 10MB limit
        var formFile = CreateMockFormFile("large.pdf", "application/pdf", 15 * 1024 * 1024);
        var request = new DocumentUploadRequest
        {
            File = formFile,
            DocumentType = DocumentType.Resume
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _documentService.UploadDocumentAsync(1, request));
    }

    [Fact]
    public async Task UploadDocumentAsync_TotalSizeExceedsLimit_ThrowsArgumentException()
    {
        // Arrange - Add existing documents to reach near the limit
        var existingDocument = new ApplicationDocument
        {
            Id = 1,
            JobApplicationId = 1,
            DocumentType = DocumentType.Portfolio,
            OriginalFileName = "existing.pdf",
            MimeType = "application/pdf",
            FileSize = 45 * 1024 * 1024, // 45MB
            GcsBucket = "test-bucket",
            GcsObjectName = "existing",
            GcsUri = "gs://test-bucket/existing"
        };
        _context.ApplicationDocuments.Add(existingDocument);
        await _context.SaveChangesAsync();

        var formFile = CreateMockFormFile("new.pdf", "application/pdf", 10 * 1024 * 1024); // 10MB
        var request = new DocumentUploadRequest
        {
            File = formFile,
            DocumentType = DocumentType.Resume
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => 
            _documentService.UploadDocumentAsync(1, request));
    }

    [Fact]
    public async Task GetDownloadUrlAsync_ExistingDocument_ReturnsDownloadResponse()
    {
        // Arrange
        var document = new ApplicationDocument
        {
            Id = 1,
            JobApplicationId = 1,
            DocumentType = DocumentType.Resume,
            OriginalFileName = "resume.pdf",
            MimeType = "application/pdf",
            FileSize = 1024,
            GcsBucket = "test-bucket",
            GcsObjectName = "test-object",
            GcsUri = "gs://test-bucket/test-object"
        };
        _context.ApplicationDocuments.Add(document);
        await _context.SaveChangesAsync();

        _mockUploadServiceClient
            .Setup(x => x.GetDownloadUrlAsync("test-bucket", "test-object", It.IsAny<TimeSpan>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://download.example.com/signed-url");

        // Act
        var result = await _documentService.GetDownloadUrlAsync(1);

        // Assert
        result.Should().NotBeNull();
        result!.DownloadUrl.Should().Be("https://download.example.com/signed-url");
        result.OriginalFileName.Should().Be("resume.pdf");
        result.MimeType.Should().Be("application/pdf");
        result.FileSize.Should().Be(1024);
    }

    [Fact]
    public async Task GetDownloadUrlAsync_NonExistentDocument_ReturnsNull()
    {
        // Act
        var result = await _documentService.GetDownloadUrlAsync(999);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_ExistingDocument_DeletesAndReturnsTrue()
    {
        // Arrange
        var document = new ApplicationDocument
        {
            Id = 1,
            JobApplicationId = 1,
            DocumentType = DocumentType.Resume,
            OriginalFileName = "resume.pdf",
            MimeType = "application/pdf",
            FileSize = 1024,
            GcsBucket = "test-bucket",
            GcsObjectName = "test-object",
            GcsUri = "gs://test-bucket/test-object"
        };
        _context.ApplicationDocuments.Add(document);
        await _context.SaveChangesAsync();

        _mockUploadServiceClient
            .Setup(x => x.DeleteFileAsync("test-bucket", "test-object", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        // Act
        var result = await _documentService.DeleteAsync(1);

        // Assert
        result.Should().BeTrue();
        
        var documentInDb = await _context.ApplicationDocuments.FirstOrDefaultAsync(d => d.Id == 1);
        documentInDb.Should().BeNull();

        // Verify UploadService was called to delete the file
        _mockUploadServiceClient.Verify(
            x => x.DeleteFileAsync("test-bucket", "test-object", It.IsAny<CancellationToken>()), 
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_NonExistentDocument_ReturnsFalse()
    {
        // Act
        var result = await _documentService.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(DocumentType.Resume, "application/pdf", true)]
    [InlineData(DocumentType.CoverLetter, "text/plain", true)]
    [InlineData(DocumentType.Portfolio, "image/jpeg", true)]
    [InlineData(DocumentType.Certificate, "image/png", true)]
    [InlineData(DocumentType.Transcript, "application/zip", true)]
    [InlineData("InvalidType", "application/pdf", false)]
    [InlineData(DocumentType.Resume, "application/msword", false)]
    [InlineData(DocumentType.Resume, "text/html", false)]
    public async Task ValidateDocumentTypeAsync_VariousInputs_ReturnsExpectedResult(
        string documentType, string mimeType, bool expected)
    {
        // Act
        var result = await _documentService.ValidateDocumentTypeAsync(documentType, mimeType);

        // Assert
        result.Should().Be(expected);
    }

    [Theory]
    [InlineData(1024, true)]
    [InlineData(10 * 1024 * 1024, true)] // 10MB - at limit
    [InlineData(11 * 1024 * 1024, false)] // 11MB - exceeds limit
    [InlineData(0, false)]
    [InlineData(-1, false)]
    public async Task ValidateFileSizeAsync_VariousSizes_ReturnsExpectedResult(long fileSize, bool expected)
    {
        // Act
        var result = await _documentService.ValidateFileSizeAsync(fileSize);

        // Assert
        result.Should().Be(expected);
    }

    [Fact]
    public async Task GetTotalFileSizeByApplicationAsync_ExistingDocuments_ReturnsCorrectSum()
    {
        // Arrange
        var documents = new[]
        {
            new ApplicationDocument
            {
                Id = 1,
                JobApplicationId = 1,
                DocumentType = DocumentType.Resume,
                OriginalFileName = "resume.pdf",
                MimeType = "application/pdf",
                FileSize = 1024,
                GcsBucket = "test-bucket",
                GcsObjectName = "resume",
                GcsUri = "gs://test-bucket/resume"
            },
            new ApplicationDocument
            {
                Id = 2,
                JobApplicationId = 1,
                DocumentType = DocumentType.CoverLetter,
                OriginalFileName = "cover.pdf",
                MimeType = "application/pdf",
                FileSize = 2048,
                GcsBucket = "test-bucket",
                GcsObjectName = "cover",
                GcsUri = "gs://test-bucket/cover"
            }
        };

        _context.ApplicationDocuments.AddRange(documents);
        await _context.SaveChangesAsync();

        // Act
        var result = await _documentService.GetTotalFileSizeByApplicationAsync(1);

        // Assert
        result.Should().Be(3072); // 1024 + 2048
    }

    [Fact]
    public async Task GetTotalFileSizeByApplicationAsync_NoDocuments_ReturnsZero()
    {
        // Act
        var result = await _documentService.GetTotalFileSizeByApplicationAsync(999);

        // Assert
        result.Should().Be(0);
    }

    [Fact]
    public async Task UploadDocumentAsync_UploadServiceFailure_ThrowsException()
    {
        // Arrange
        var formFile = CreateMockFormFile("test.pdf", "application/pdf", 1024);
        var request = new DocumentUploadRequest
        {
            File = formFile,
            DocumentType = DocumentType.Resume
        };

        _mockUploadServiceClient
            .Setup(x => x.UploadFileAsync(It.IsAny<Stream>(), It.IsAny<UploadServiceRequest>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Upload service unavailable"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => 
            _documentService.UploadDocumentAsync(1, request));

        // Verify no database record was created
        var documentsInDb = await _context.ApplicationDocuments.Where(d => d.JobApplicationId == 1).ToListAsync();
        documentsInDb.Should().BeEmpty();
    }

    private static IFormFile CreateMockFormFile(string fileName, string contentType, long length)
    {
        var formFile = new Mock<IFormFile>();
        formFile.Setup(f => f.FileName).Returns(fileName);
        formFile.Setup(f => f.ContentType).Returns(contentType);
        formFile.Setup(f => f.Length).Returns(length);
        formFile.Setup(f => f.OpenReadStream()).Returns(new MemoryStream(new byte[length]));
        return formFile.Object;
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}