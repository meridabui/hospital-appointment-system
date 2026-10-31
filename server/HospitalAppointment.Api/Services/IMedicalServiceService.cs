using HospitalAppointment.Api.DTOs.MedicalService;

namespace HospitalAppointment.Api.Services;

public interface IMedicalServiceService
{
    // 1. Lấy danh sách, tìm kiếm và lọc
    Task<List<MedicalServiceDto>> GetAllAsync(
        string? keyword = null,
        int? departmentId = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    // 2. Lấy dịch vụ đang hoạt động
    Task<List<MedicalServiceDto>> GetActiveAsync(
        int? departmentId = null,
        CancellationToken cancellationToken = default);

    // 3. Lấy chi tiết dịch vụ
    Task<MedicalServiceDto> GetByIdAsync(
        int medicalServiceId,
        CancellationToken cancellationToken = default);

    // 4. Tạo dịch vụ
    Task<MedicalServiceDto> CreateAsync(
        CreateMedicalServiceDto request,
        CancellationToken cancellationToken = default);

    // 5. Cập nhật dịch vụ
    Task<MedicalServiceDto> UpdateAsync(
        int medicalServiceId,
        UpdateMedicalServiceDto request,
        CancellationToken cancellationToken = default);

    // 6. Kích hoạt / vô hiệu hóa
    Task<MedicalServiceDto> UpdateStatusAsync(
        int medicalServiceId,
        bool isActive,
        CancellationToken cancellationToken = default);
}