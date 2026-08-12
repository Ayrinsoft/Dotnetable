using Dotnetable.Domain.Entities;

namespace Dotnetable.Application.Interfaces;

public interface IHrService
{
    Task<IReadOnlyList<OrgUnit>> GetOrgUnitsAsync(int websiteId, CancellationToken ct = default);
    Task<OrgUnit> UpsertOrgUnitAsync(OrgUnit unit, CancellationToken ct = default);
    Task<IReadOnlyList<Employee>> GetEmployeesAsync(int websiteId, CancellationToken ct = default);
    Task<Employee?> GetEmployeeAsync(int employeeId, CancellationToken ct = default);
    Task<Employee> UpsertEmployeeAsync(Employee employee, CancellationToken ct = default);
    Task<EmployeeContract> UpsertContractAsync(EmployeeContract contract, CancellationToken ct = default);
    Task<EmployeeContract?> GetActiveContractAsync(int employeeId, DateOnly asOf, CancellationToken ct = default);
}
