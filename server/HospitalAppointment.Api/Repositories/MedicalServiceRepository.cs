using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public class MedicalServiceRepository : IMedicalServiceRepository
{
    private readonly HospitalAppointmentDbContext _context;

    public MedicalServiceRepository(
        HospitalAppointmentDbContext context)
    {
        _context = context;
    }

    // 1. GET ALL / SEARCH / FILTER

    public async Task<List<MedicalService>> GetAllAsync(
        string? keyword = null,
        int? departmentId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MedicalServices
            .AsNoTracking()
            .Include(s => s.Department)
            .AsQueryable();

        // Tìm kiếm theo mã hoặc tên dịch vụ
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();

            query = query.Where(s =>
                s.ServiceCode.Contains(keyword) ||
                s.ServiceName.Contains(keyword));
        }

        // Lọc theo Department
        if (departmentId.HasValue)
        {
            query = query.Where(
                s => s.DepartmentId == departmentId.Value);
        }

        // Lọc theo trạng thái
        if (isActive.HasValue)
        {
            query = query.Where(
                s => s.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(s => s.Department.DepartmentName)
            .ThenBy(s => s.ServiceName)
            .ToListAsync(cancellationToken);
    }

    // 2. GET ACTIVE MEDICAL SERVICES

    public async Task<List<MedicalService>> GetActiveAsync(
        int? departmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MedicalServices
            .AsNoTracking()
            .Include(s => s.Department)
            .Where(s => s.IsActive && s.Department.IsActive);

        if (departmentId.HasValue)
        {
            query = query.Where(
                s => s.DepartmentId == departmentId.Value);
        }

        return await query
            .OrderBy(s => s.ServiceName)
            .ToListAsync(cancellationToken);
    }

    // 3. GET BY ID

    public async Task<MedicalService?> GetByIdAsync(
        int medicalServiceId,
        CancellationToken cancellationToken = default)
    {
        return await _context.MedicalServices
            .Include(s => s.Department)
            .FirstOrDefaultAsync(
                s => s.MedicalServiceId == medicalServiceId,
                cancellationToken);
    }

    // 4. CHECK SERVICE CODE
    public async Task<bool> ExistsByCodeAsync(
        string serviceCode,
        int? excludeMedicalServiceId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.MedicalServices
            .AsNoTracking()
            .Where(s => s.ServiceCode == serviceCode);

        if (excludeMedicalServiceId.HasValue)
        {
            query = query.Where(
                s => s.MedicalServiceId != excludeMedicalServiceId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    // 5. ADD MEDICAL SERVICE
    public async Task AddAsync(
        MedicalService medicalService,
        CancellationToken cancellationToken = default)
    {
        await _context.MedicalServices.AddAsync(
            medicalService,
            cancellationToken);
    }

    // 6. SAVE CHANGES
    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}