using HospitalAppointment.Api.DTOs.Department;

namespace HospitalAppointment.Api.Services;

public interface IDepartmentService
{
    // 1. Lấy danh sách, tìm kiếm và lọc
    Task<List<DepartmentDto>> GetAllAsync(
        string? keyword = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    // 2. Lấy danh sách khoa đang hoạt động
    Task<List<DepartmentDto>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    // 3. Lấy chi tiết Department
    Task<DepartmentDto> GetByIdAsync(
        int departmentId,
        CancellationToken cancellationToken = default);

    // 4. Tạo Department
    Task<DepartmentDto> CreateAsync(
        CreateDepartmentDto request,
        CancellationToken cancellationToken = default);

    // 5. Cập nhật Department
    Task<DepartmentDto> UpdateAsync(
        int departmentId,
        UpdateDepartmentDto request,
        CancellationToken cancellationToken = default);

    // 6. Kích hoạt / vô hiệu hóa
    Task<DepartmentDto> UpdateStatusAsync(
        int departmentId,
        bool isActive,
        CancellationToken cancellationToken = default);
}