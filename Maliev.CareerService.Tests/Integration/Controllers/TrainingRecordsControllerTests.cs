using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using Maliev.CareerService.Api.Models.TrainingRecords;
using Maliev.CareerService.Data.Enums;
using Xunit;

namespace Maliev.CareerService.Tests.Integration.Controllers;

public class TrainingRecordsControllerTests : IntegrationTestBase
{
    public TrainingRecordsControllerTests(CareerServiceWebApplicationFactory factory) : base(factory)
    {
    }

    [Fact]
    public async Task Post_CreateTrainingRecord_ShouldReturnCreated()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateHRStaffToken(employeeId); // HR can create records
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        var request = new RecordTrainingCompletionRequest
        {
            CourseName = "Advanced C#",
            CompletionDate = DateTime.UtcNow.AddDays(-1),
            TrainingType = TrainingType.Certification,
            Provider = "Udemy",
            Score = 95
        };

        // Act
        var response = await Client.PostAsJsonAsync($"/career/v1/employees/{employeeId}/training-records", request);

        // Assert
        if (response.StatusCode != HttpStatusCode.Created)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"Post failed with {response.StatusCode}: {error}");
        }
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        var createdRecord = await response.Content.ReadFromJsonAsync<TrainingRecordResponse>();
        createdRecord.Should().NotBeNull();
        createdRecord!.CourseName.Should().Be(request.CourseName);
        createdRecord.EmployeeId.Should().Be(employeeId);
    }

    [Fact]
    public async Task Get_GetEmployeeTrainingRecords_ShouldReturnList()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var token = GenerateEmployeeToken(employeeId); // Employee can view their own
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Create a record first (assuming we can use the same flow or seed DB)
        // For integration tests, it's better to seed DB directly if possible, or use the API if we trust it.
        // Since we just tested POST, let's use POST to set up data or assume SeedDatabase works if we had the entity access.
        // Let's use the API to create one first.
        var hrToken = GenerateHRStaffToken(Guid.NewGuid());
        var request = new HttpRequestMessage(HttpMethod.Post, $"/career/v1/employees/{employeeId}/training-records");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);
        request.Content = JsonContent.Create(new RecordTrainingCompletionRequest
        {
            CourseName = "Integration Test Course",
            CompletionDate = DateTime.UtcNow.AddDays(-5),
            TrainingType = TrainingType.Workshop
        });
        await Client.SendAsync(request);

        // Act
        var response = await Client.GetAsync($"/career/v1/employees/{employeeId}/training-records");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var records = await response.Content.ReadFromJsonAsync<TrainingRecordListResponse>();
        records.Should().NotBeNull();
        records!.Items.Should().Contain(r => r.CourseName == "Integration Test Course");
    }

    [Fact]
    public async Task Get_GetTrainingRecordById_ShouldReturnRecord()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var hrToken = GenerateHRStaffToken(Guid.NewGuid());

        // Create record
        var createRequest = new RecordTrainingCompletionRequest
        {
            CourseName = "Specific Record Test",
            CompletionDate = DateTime.UtcNow.AddDays(-2),
            TrainingType = TrainingType.Workshop
        };
        var createMsg = new HttpRequestMessage(HttpMethod.Post, $"/career/v1/employees/{employeeId}/training-records");
        createMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);
        createMsg.Content = JsonContent.Create(createRequest);

        var createResponse = await Client.SendAsync(createMsg);
        var createdRecord = await createResponse.Content.ReadFromJsonAsync<TrainingRecordResponse>();
        var recordId = createdRecord!.Id;

        var token = GenerateEmployeeToken(employeeId);
        Client.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await Client.GetAsync($"/career/v1/employees/{employeeId}/training-records/{recordId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var record = await response.Content.ReadFromJsonAsync<TrainingRecordResponse>();
        record.Should().NotBeNull();
        record!.Id.Should().Be(recordId);
        record.CourseName.Should().Be("Specific Record Test");
    }

    [Fact]
    public async Task Put_UpdateTrainingRecord_ShouldUpdateAndReturnOk()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var hrToken = GenerateHRStaffToken(Guid.NewGuid());

        // Create record
        var createRequest = new RecordTrainingCompletionRequest
        {
            CourseName = "To Be Updated",
            CompletionDate = DateTime.UtcNow.AddDays(-10),
            TrainingType = TrainingType.External
        };
        var createMsg = new HttpRequestMessage(HttpMethod.Post, $"/career/v1/employees/{employeeId}/training-records");
        createMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);
        createMsg.Content = JsonContent.Create(createRequest);

        var createResponse = await Client.SendAsync(createMsg);
        var createdRecord = await createResponse.Content.ReadFromJsonAsync<TrainingRecordResponse>();
        var recordId = createdRecord!.Id;

        // Update request
        var updateRequest = new UpdateTrainingRecordRequest
        {
            CourseName = "Updated Course Name",
            CompletionDate = DateTime.UtcNow.AddDays(-10),
            TrainingType = TrainingType.External,
            Score = 100
        };

        var updateMsg = new HttpRequestMessage(HttpMethod.Put, $"/career/v1/employees/{employeeId}/training-records/{recordId}");
        updateMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);
        updateMsg.Content = JsonContent.Create(updateRequest);

        // Act
        var response = await Client.SendAsync(updateMsg);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var updatedRecord = await response.Content.ReadFromJsonAsync<TrainingRecordResponse>();
        updatedRecord.Should().NotBeNull();
        updatedRecord!.CourseName.Should().Be("Updated Course Name");
        updatedRecord.Score.Should().Be(100);
    }

    [Fact]
    public async Task Get_GetExpiringRecords_ShouldReturnExpiringRecords()
    {
        // Arrange
        var employeeId = Guid.NewGuid();
        var hrToken = GenerateHRStaffToken(Guid.NewGuid());

        // Create an expiring record (in 30 days)
        var expiringRequest = new RecordTrainingCompletionRequest
        {
            CourseName = "Expiring Course",
            CompletionDate = DateTime.UtcNow.AddDays(-335),
            ExpirationDate = DateTime.UtcNow.AddDays(30),
            TrainingType = TrainingType.Certification
        };
        var createMsg = new HttpRequestMessage(HttpMethod.Post, $"/career/v1/employees/{employeeId}/training-records");
        createMsg.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);
        createMsg.Content = JsonContent.Create(expiringRequest);
        await Client.SendAsync(createMsg);

        // Act
        var request = new HttpRequestMessage(HttpMethod.Get, "/career/v1/training-records/expiring?days=60");
        request.Headers.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", hrToken);
        var response = await Client.SendAsync(request);

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
        var records = await response.Content.ReadFromJsonAsync<TrainingRecordListResponse>();
        records.Should().NotBeNull();
        records!.Items.Should().Contain(r => r.CourseName == "Expiring Course");
    }
}
