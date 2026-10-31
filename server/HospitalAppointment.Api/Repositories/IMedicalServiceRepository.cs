using HospitalAppointment.Api.Models.Entities;

namespace HospitalAppointment.Api.Repositories;

public interface IMedicalServiceRepository
{
    // Lấy danh sách dịch vụ, tìm kiếm và lọc
    Task<List<MedicalService>> GetAllAsync(
        string? keyword = null,
        int? departmentId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    // Lấy dịch vụ đang hoạt động
    Task<List<MedicalService>> GetActiveAsync(
        int? departmentId = null,
        CancellationToken cancellationToken = default);

    // Lấy dịch vụ theo ID
    Task<MedicalService?> GetByIdAsync(
        int medicalServiceId,
        CancellationToken cancellationToken = default);

    // Kiểm tra ServiceCode đã tồn tại
    Task<bool> ExistsByCodeAsync(
        string serviceCode,
        int? excludeMedicalServiceId = null,
        CancellationToken cancellationToken = default);

    // Thêm dịch vụ
    Task AddAsync(
        MedicalService medicalService,
        CancellationToken cancellationToken = default);

    // Lưu thay đổi
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}