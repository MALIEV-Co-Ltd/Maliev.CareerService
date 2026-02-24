using Maliev.CareerService.Api.Services.External;

namespace Maliev.CareerService.Tests.Mocks;

/// <summary>
/// Mock implementation of IEmployeeServiceClient for testing
/// </summary>
public class MockEmployeeServiceClient : IEmployeeServiceClient
{
    private readonly Dictionary<Guid, EmployeeResponse> _employees = [];

    public MockEmployeeServiceClient()
    {
        // Add some test employees
        _employees[Guid.Parse("11111111-1111-1111-1111-111111111111")] = new EmployeeResponse(
            Guid.Parse("11111111-1111-1111-1111-111111111111"),
            "John",
            "Doe",
            "john.doe@maliev.com",
            "Engineering",
            "Software Engineer",
            Guid.Parse("22222222-2222-2222-2222-222222222222"));

        _employees[Guid.Parse("22222222-2222-2222-2222-222222222222")] = new EmployeeResponse(
            Guid.Parse("22222222-2222-2222-2222-222222222222"),
            "Jane",
            "Smith",
            "jane.smith@maliev.com",
            "HR",
            "HR Manager",
            null);
    }

    public Task<EmployeeResponse?> GetEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        _employees.TryGetValue(employeeId, out var employee);
        return Task.FromResult(employee);
    }

    public Task<bool> ValidateEmployeeAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_employees.ContainsKey(employeeId));
    }

    public void AddEmployee(EmployeeResponse employee)
    {
        _employees[employee.Id] = employee;
    }
}
