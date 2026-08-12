using Dotnetable.Application.Interfaces;
using Dotnetable.Domain.Entities;
using Dotnetable.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Dotnetable.Infrastructure.Services;

public class HrService : IHrService
{
    private readonly AppDbContext _context;
    public HrService(AppDbContext context) => _context = context;

    public async Task<IReadOnlyList<OrgUnit>> GetOrgUnitsAsync(int websiteId, CancellationToken ct = default) =>
        await _context.OrgUnits.AsNoTracking()
            .Where(o => o.WebsiteID == websiteId)
            .OrderBy(o => o.SortOrder).ThenBy(o => o.Name)
            .ToListAsync(ct);

    public async Task<OrgUnit> UpsertOrgUnitAsync(OrgUnit unit, CancellationToken ct = default)
    {
        if (unit.OrgUnitID == 0) _context.OrgUnits.Add(unit);
        else
        {
            var e = await _context.OrgUnits.FirstAsync(x => x.OrgUnitID == unit.OrgUnitID, ct);
            e.Code = unit.Code; e.Name = unit.Name; e.ParentOrgUnitID = unit.ParentOrgUnitID;
            e.SortOrder = unit.SortOrder; e.IsActive = unit.IsActive;
            unit = e;
        }
        await _context.SaveChangesAsync(ct);
        return unit;
    }

    public async Task<IReadOnlyList<Employee>> GetEmployeesAsync(int websiteId, CancellationToken ct = default) =>
        await _context.Employees.AsNoTracking()
            .Include(e => e.OrgUnit)
            .Where(e => e.WebsiteID == websiteId)
            .OrderBy(e => e.Surname).ThenBy(e => e.GivenName)
            .ToListAsync(ct);

    public async Task<Employee?> GetEmployeeAsync(int employeeId, CancellationToken ct = default) =>
        await _context.Employees.AsNoTracking()
            .Include(e => e.OrgUnit)
            .Include(e => e.EmployeeContracts)
            .FirstOrDefaultAsync(e => e.EmployeeID == employeeId, ct);

    public async Task<Employee> UpsertEmployeeAsync(Employee employee, CancellationToken ct = default)
    {
        if (employee.EmployeeID == 0)
        {
            employee.CreatedAt = DateTime.UtcNow;
            _context.Employees.Add(employee);
        }
        else
        {
            var e = await _context.Employees.FirstAsync(x => x.EmployeeID == employee.EmployeeID, ct);
            e.EmployeeCode = employee.EmployeeCode;
            e.GivenName = employee.GivenName;
            e.Surname = employee.Surname;
            e.NationalId = employee.NationalId;
            e.Email = employee.Email;
            e.Phone = employee.Phone;
            e.OrgUnitID = employee.OrgUnitID;
            e.JobTitle = employee.JobTitle;
            e.MemberID = employee.MemberID;
            e.Status = employee.Status;
            e.HireDate = employee.HireDate;
            e.TerminationDate = employee.TerminationDate;
            employee = e;
        }
        await _context.SaveChangesAsync(ct);
        return employee;
    }

    public async Task<EmployeeContract> UpsertContractAsync(EmployeeContract contract, CancellationToken ct = default)
    {
        if (contract.IsActive)
        {
            var others = await _context.EmployeeContracts
                .Where(c => c.EmployeeID == contract.EmployeeID && c.IsActive && c.EmployeeContractID != contract.EmployeeContractID)
                .ToListAsync(ct);
            foreach (var o in others) o.IsActive = false;
        }
        if (contract.EmployeeContractID == 0)
        {
            contract.CreatedAt = DateTime.UtcNow;
            _context.EmployeeContracts.Add(contract);
        }
        else
        {
            var e = await _context.EmployeeContracts.FirstAsync(c => c.EmployeeContractID == contract.EmployeeContractID, ct);
            e.EffectiveFrom = contract.EffectiveFrom;
            e.EffectiveTo = contract.EffectiveTo;
            e.BaseSalary = contract.BaseSalary;
            e.CurrencyCode = contract.CurrencyCode;
            e.PayFrequency = contract.PayFrequency;
            e.EmployeeInsuranceRate = contract.EmployeeInsuranceRate;
            e.EmployerInsuranceRate = contract.EmployerInsuranceRate;
            e.IncomeTaxRate = contract.IncomeTaxRate;
            e.IsActive = contract.IsActive;
            contract = e;
        }
        await _context.SaveChangesAsync(ct);
        return contract;
    }

    public async Task<EmployeeContract?> GetActiveContractAsync(int employeeId, DateOnly asOf, CancellationToken ct = default) =>
        await _context.EmployeeContracts.AsNoTracking()
            .Where(c => c.EmployeeID == employeeId && c.IsActive
                        && c.EffectiveFrom <= asOf
                        && (c.EffectiveTo == null || c.EffectiveTo >= asOf))
            .OrderByDescending(c => c.EffectiveFrom)
            .FirstOrDefaultAsync(ct);
}
