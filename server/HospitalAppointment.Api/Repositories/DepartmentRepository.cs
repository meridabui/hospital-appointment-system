using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public class DepartmentRepository : IDepartmentRepository
{
    private readonly HospitalAppointmentDbContext _context;

    public DepartmentRepository(HospitalAppointmentDbContext context)
    {
        _context = context;
    }

    // 1. Lấy danh sách Department
    // Hỗ trợ tìm kiếm và lọc trạng thái
    public async Task<List<Department>> GetAllAsync(
        string? keyword = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Departments.AsNoTracking();

        // Tìm kiếm theo mã hoặc tên khoa
        if (!string.IsNullOrWhiteSpace(keyword))
        {
            keyword = keyword.Trim();

            query = query.Where(d =>
                d.DepartmentCode.Contains(keyword) ||
                d.DepartmentName.Contains(keyword));
        }

        // Lọc theo trạng thái
        if (isActive.HasValue)
        {
            query = query.Where(d => d.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(d => d.DepartmentName)
            .ToListAsync(cancellationToken);
    }

    // 2. Lấy danh sách Department đang hoạt động
    public async Task<List<Department>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .AsNoTracking()
            .Where(d => d.IsActive)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync(cancellationToken);
    }

    // 3. Lấy Department theo ID
    public async Task<Department?> GetByIdAsync(
        int departmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Departments
            .FirstOrDefaultAsync(
                d => d.DepartmentId == departmentId,
                cancellationToken);
    }

    // 4. Kiểm tra DepartmentCode trùng
    public async Task<bool> ExistsByCodeAsync(
        string departmentCode,
        int? excludeDepartmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Departments
            .AsNoTracking()
            .Where(d => d.DepartmentCode == departmentCode);

        if (excludeDepartmentId.HasValue)
        {
            query = query.Where(
                d => d.DepartmentId != excludeDepartmentId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    // 5. Kiểm tra DepartmentName trùng
    public async Task<bool> ExistsByNameAsync(
        string departmentName,
        int? excludeDepartmentId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.Departments
            .AsNoTracking()
            .Where(d => d.DepartmentName == departmentName);

        if (excludeDepartmentId.HasValue)
        {
            query = query.Where(
                d => d.DepartmentId != excludeDepartmentId.Value);
        }

        return await query.AnyAsync(cancellationToken);
    }

    // 6. Thêm Department
    public async Task AddAsync(
        Department department,
        CancellationToken cancellationToken = default)
    {
        await _context.Departments.AddAsync(
            department,
            cancellationToken);
    }

    // 7. Lưu thay đổi
    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}