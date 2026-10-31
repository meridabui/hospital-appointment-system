using HospitalAppointment.Api.DTOs.Department;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Repositories;

namespace HospitalAppointment.Api.Services;

public class DepartmentService : IDepartmentService
{
    private readonly IDepartmentRepository _departmentRepository;

    public DepartmentService(
        IDepartmentRepository departmentRepository)
    {
        _departmentRepository = departmentRepository;
    }

    // 1. GET ALL / SEARCH / FILTER

    public async Task<List<DepartmentDto>> GetAllAsync(
        string? keyword = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var departments = await _departmentRepository.GetAllAsync(
            keyword,
            isActive,
            cancellationToken);

        return departments.Select(MapToDto).ToList();
    }

    // 2. GET ACTIVE DEPARTMENTS

    public async Task<List<DepartmentDto>> GetActiveAsync(
        CancellationToken cancellationToken = default)
    {
        var departments =
            await _departmentRepository.GetActiveAsync(
                cancellationToken);

        return departments.Select(MapToDto).ToList();
    }

    // 3. GET BY ID

    public async Task<DepartmentDto> GetByIdAsync(
        int departmentId,
        CancellationToken cancellationToken = default)
    {
        if (departmentId <= 0)
            throw new ArgumentException("Invalid department ID.");

        var department = await _departmentRepository.GetByIdAsync(
            departmentId,
            cancellationToken);

        if (department == null)
            throw new KeyNotFoundException("Department not found.");

        return MapToDto(department);
    }

    // 4. CREATE DEPARTMENT


    public async Task<DepartmentDto> CreateAsync(
        CreateDepartmentDto request,
        CancellationToken cancellationToken = default)
    {
        var code = request.DepartmentCode.Trim().ToUpperInvariant();
        var name = request.DepartmentName.Trim();

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Department code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Department name is required.");

        // Kiểm tra mã khoa trùng
        if (await _departmentRepository.ExistsByCodeAsync(
            code,
            cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException(
                "Department code already exists.");
        }

        // Kiểm tra tên khoa trùng
        if (await _departmentRepository.ExistsByNameAsync(
            name,
            cancellationToken: cancellationToken))
        {
            throw new InvalidOperationException(
                "Department name already exists.");
        }

        var department = new Department
        {
            DepartmentCode = code,
            DepartmentName = name,
            Description = request.Description?.Trim(),
            IsActive = true
        };

        await _departmentRepository.AddAsync(
            department,
            cancellationToken);

        await _departmentRepository.SaveChangesAsync(
            cancellationToken);

        return MapToDto(department);
    }

    // 5. UPDATE DEPARTMENT

    public async Task<DepartmentDto> UpdateAsync(
        int departmentId,
        UpdateDepartmentDto request,
        CancellationToken cancellationToken = default)
    {
        if (departmentId <= 0)
            throw new ArgumentException("Invalid department ID.");

        var department = await _departmentRepository.GetByIdAsync(
            departmentId,
            cancellationToken);

        if (department == null)
            throw new KeyNotFoundException("Department not found.");

        var code = request.DepartmentCode.Trim().ToUpperInvariant();
        var name = request.DepartmentName.Trim();

        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Department code is required.");

        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Department name is required.");

        // Kiểm tra mã trùng với Department khác
        if (await _departmentRepository.ExistsByCodeAsync(
            code,
            departmentId,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "Department code already exists.");
        }

        // Kiểm tra tên trùng với Department khác
        if (await _departmentRepository.ExistsByNameAsync(
            name,
            departmentId,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "Department name already exists.");
        }

        department.DepartmentCode = code;
        department.DepartmentName = name;
        department.Description = request.Description?.Trim();

        await _departmentRepository.SaveChangesAsync(
            cancellationToken);

        return MapToDto(department);
    }

    // 6. ACTIVATE / DEACTIVATE

    public async Task<DepartmentDto> UpdateStatusAsync(
        int departmentId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        if (departmentId <= 0)
            throw new ArgumentException("Invalid department ID.");

        var department = await _departmentRepository.GetByIdAsync(
            departmentId,
            cancellationToken);

        if (department == null)
            throw new KeyNotFoundException("Department not found.");

        department.IsActive = isActive;

        await _departmentRepository.SaveChangesAsync(
            cancellationToken);

        return MapToDto(department);
    }

    // ENTITY -> DTO

    private static DepartmentDto MapToDto(Department department)
    {
        return new DepartmentDto
        {
            DepartmentId = department.DepartmentId,
            DepartmentCode = department.DepartmentCode,
            DepartmentName = department.DepartmentName,
            Description = department.Description,
            IsActive = department.IsActive,
            CreatedAt = department.CreatedAt
        };
    }
}