using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public interface IDepartmentRepository
{
    Task<List<Department>> GetActiveAsync();
    Task<Department?> GetByIdAsync(int departmentId);
}

public class DepartmentRepository : IDepartmentRepository
{
    private readonly HospitalAppointmentDbContext _db;
    public DepartmentRepository(HospitalAppointmentDbContext db) => _db = db;

    public Task<List<Department>> GetActiveAsync()
        => _db.Departments.AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync();

    public Task<Department?> GetByIdAsync(int departmentId)
        => _db.Departments.AsNoTracking().FirstOrDefaultAsync(d => d.DepartmentId == departmentId);
}