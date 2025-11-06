using FluentAssertions;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Readers;
using System.Net;
using System.Net.Http.Json;
using Xunit;
using Maliev.CareerService.Tests.Helpers;

namespace Maliev.CareerService.Tests.Contract;

/// <summary>
/// Contract tests verifying IDP endpoints comply with OpenAPI specification
/// </summary>
public class IDPContractTests(CareerServiceWebApplicationFactory factory) : IntegrationTestBase(factory)
{
    private OpenApiDocument? _openApiSpec;

    private async Task<OpenApiDocument> GetOpenApiSpecAsync()
    {
        if (_openApiSpec != null)
            return _openApiSpec;

        var response = await Client.GetAsync("/career/swagger/v1/swagger.json");
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStreamAsync();
        var reader = new OpenApiStreamReader();
        _openApiSpec = reader.Read(content, out var diagnostic);

        diagnostic.Errors.Should().BeEmpty("OpenAPI spec should be valid");
        return _openApiSpec;
    }

    [DockerRequiredFact]
    public async Task OpenAPISpec_ContainsIDPEndpoints()
    {
        // Arrange
        var spec = await GetOpenApiSpecAsync();

        // Assert - Verify IDP endpoints exist
        spec.Paths.Should().ContainKey("/careers/v1/idps");
        spec.Paths.Should().ContainKey("/careers/v1/idps/{id}");
        spec.Paths.Should().ContainKey("/careers/v1/idps/{id}/submit");
        spec.Paths.Should().ContainKey("/careers/v1/idps/{id}/approve");
    }

    [DockerRequiredFact]
    public async Task OpenAPISpec_ContainsDevelopmentGoalEndpoints()
    {
        // Arrange
        var spec = await GetOpenApiSpecAsync();

        // Assert - Verify Development Goal endpoints exist
        spec.Paths.Should().ContainKey("/careers/v1/idps/{idpId}/goals");
        spec.Paths.Should().ContainKey("/careers/v1/goals/{id}");
        spec.Paths.Should().ContainKey("/careers/v1/goals/{id}/status");
    }

    [DockerRequiredFact]
    public async Task OpenAPISpec_IDPEndpoint_HasGetMethod()
    {
        // Arrange
        var spec = await GetOpenApiSpecAsync();

        // Assert
        var path = spec.Paths["/careers/v1/idps"];
        path.Operations.Should().ContainKey(OperationType.Get);

        var getOp = path.Operations[OperationType.Get];
        getOp.Responses.Should().ContainKey("200");
        getOp.Responses["200"].Content.Should().ContainKey("application/json");
    }

    [DockerRequiredFact]
    public async Task OpenAPISpec_IDPEndpoint_HasPostMethod()
    {
        // Arrange
        var spec = await GetOpenApiSpecAsync();

        // Assert
        var path = spec.Paths["/careers/v1/idps"];
        path.Operations.Should().ContainKey(OperationType.Post);

        var postOp = path.Operations[OperationType.Post];
        postOp.Responses.Should().ContainKey("201");
        postOp.RequestBody.Should().NotBeNull();
        postOp.RequestBody.Content.Should().ContainKey("application/json");
    }

    [DockerRequiredFact]
    public async Task OpenAPISpec_IDPUpdateEndpoint_HasPutMethod()
    {
        // Arrange
        var spec = await GetOpenApiSpecAsync();

        // Assert
        var path = spec.Paths["/careers/v1/idps/{id}"];
        path.Operations.Should().ContainKey(OperationType.Put);

        var putOp = path.Operations[OperationType.Put];
        putOp.Parameters.Should().Contain(p => p.Name == "id");
        putOp.Responses.Should().ContainKey("200");
    }

    [DockerRequiredFact]
    public async Task OpenAPISpec_IDPSubmitEndpoint_HasPatchMethod()
    {
        // Arrange
        var spec = await GetOpenApiSpecAsync();

        // Assert
        var path = spec.Paths["/careers/v1/idps/{id}/submit"];
        path.Operations.Should().ContainKey(OperationType.Patch);

        var patchOp = path.Operations[OperationType.Patch];
        patchOp.Parameters.Should().Contain(p => p.Name == "id");
        patchOp.Responses.Should().ContainKey("200");
    }

    [DockerRequiredFact]
    public async Task OpenAPISpec_IDPApproveEndpoint_HasPatchMethod()
    {
        // Arrange
        var spec = await GetOpenApiSpecAsync();

        // Assert
        var path = spec.Paths["/careers/v1/idps/{id}/approve"];
        path.Operations.Should().ContainKey(OperationType.Patch);

        var patchOp = path.Operations[OperationType.Patch];
        patchOp.Parameters.Should().Contain(p => p.Name == "id");
        patchOp.Responses.Should().ContainKey("200");
        patchOp.RequestBody.Should().NotBeNull();
    }

    [DockerRequiredFact]
    public async Task OpenAPISpec_CreateGoalEndpoint_HasPostMethod()
    {
        // Arrange
        var spec = await GetOpenApiSpecAsync();

        // Assert
        var path = spec.Paths["/careers/v1/idps/{idpId}/goals"];
        path.Operations.Should().ContainKey(OperationType.Post);

        var postOp = path.Operations[OperationType.Post];
        postOp.Parameters.Should().Contain(p => p.Name == "idpId");
        postOp.Responses.Should().ContainKey("201");
        postOp.RequestBody.Should().NotBeNull();
    }

    [DockerRequiredFact]
    public async Task OpenAPISpec_UpdateGoalEndpoint_HasPutMethod()
    {
        // Arrange
        var spec = await GetOpenApiSpecAsync();

        // Assert
        var path = spec.Paths["/careers/v1/goals/{id}"];
        path.Operations.Should().ContainKey(OperationType.Put);

        var putOp = path.Operations[OperationType.Put];
        putOp.Parameters.Should().Contain(p => p.Name == "id");
        putOp.Responses.Should().ContainKey("200");
    }

    [DockerRequiredFact]
    public async Task OpenAPISpec_UpdateGoalStatusEndpoint_HasPatchMethod()
    {
        // Arrange
        var spec = await GetOpenApiSpecAsync();

        // Assert
        var path = spec.Paths["/careers/v1/goals/{id}/status"];
        path.Operations.Should().ContainKey(OperationType.Patch);

        var patchOp = path.Operations[OperationType.Patch];
        patchOp.Parameters.Should().Contain(p => p.Name == "id");
        patchOp.Responses.Should().ContainKey("200");
        patchOp.RequestBody.Should().NotBeNull();
    }

    [DockerRequiredFact]
    public async Task OpenAPISpec_IDPSchemas_AreDefined()
    {
        // Arrange
        var spec = await GetOpenApiSpecAsync();

        // Assert - Verify required schemas exist
        spec.Components.Schemas.Should().ContainKey("IDPResponse");
        spec.Components.Schemas.Should().ContainKey("CreateIDPRequest");
        spec.Components.Schemas.Should().ContainKey("UpdateIDPRequest");
        spec.Components.Schemas.Should().ContainKey("ApproveIDPRequest");
        spec.Components.Schemas.Should().ContainKey("IDPListResponse");
    }

    [DockerRequiredFact]
    public async Task OpenAPISpec_DevelopmentGoalSchemas_AreDefined()
    {
        // Arrange
        var spec = await GetOpenApiSpecAsync();

        // Assert - Verify required schemas exist
        spec.Components.Schemas.Should().ContainKey("DevelopmentGoalResponse");
        spec.Components.Schemas.Should().ContainKey("CreateDevelopmentGoalRequest");
        spec.Components.Schemas.Should().ContainKey("UpdateDevelopmentGoalRequest");
        spec.Components.Schemas.Should().ContainKey("UpdateGoalStatusRequest");
    }
}
