using HospitalAppointment.Api.Models.Entities;

namespace HospitalAppointment.Api.Repositories;

public interface IDepartmentRepository
{
    // Lấy danh sách Department, hỗ trợ tìm kiếm và lọc trạng thái
    Task<List<Department>> GetAllAsync(
        string? keyword = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    // Lấy danh sách Department đang hoạt động
    Task<List<Department>> GetActiveAsync(
        CancellationToken cancellationToken = default);

    // Lấy Department theo ID
    Task<Department?> GetByIdAsync(
        int departmentId,
        CancellationToken cancellationToken = default);

    // Kiểm tra DepartmentCode đã tồn tại chưa
    Task<bool> ExistsByCodeAsync(
        string departmentCode,
        int? excludeDepartmentId = null,
        CancellationToken cancellationToken = default);

    // Kiểm tra DepartmentName đã tồn tại chưa
    Task<bool> ExistsByNameAsync(
        string departmentName,
        int? excludeDepartmentId = null,
        CancellationToken cancellationToken = default);

    // Thêm Department
    Task AddAsync(
        Department department,
        CancellationToken cancellationToken = default);

    // Lưu thay đổi
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}