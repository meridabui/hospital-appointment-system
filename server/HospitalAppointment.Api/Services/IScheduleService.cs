
using HospitalAppointment.Api.DTOs.Schedule;

namespace HospitalAppointment.Api.Services;

public interface IScheduleService
{
    // 1. Lấy danh sách Schedule Slot
    Task<List<DoctorScheduleDto>> GetAllAsync(
        int? doctorId = null,
        int? departmentId = null,
        DateOnly? slotDate = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    // 2. Lấy chi tiết Schedule Slot
    Task<DoctorScheduleDto> GetByIdAsync(
        int scheduleSlotId,
        CancellationToken cancellationToken = default);

    // 3. Lấy danh sách Doctor
    Task<List<DoctorOptionDto>> GetDoctorsAsync(
        CancellationToken cancellationToken = default);

    // 4. Lấy Department được phân công cho Doctor
    Task<List<DepartmentOptionDto>> GetDepartmentsByDoctorAsync(
        int doctorId,
        CancellationToken cancellationToken = default);

    // 5. Tạo lịch làm việc và tự động sinh slot
    Task<List<DoctorScheduleDto>> CreateAsync(
        CreateScheduleDto request,
        int createdBy,
        CancellationToken cancellationToken = default);

    // 6. Chỉnh sửa Schedule Slot
    Task<DoctorScheduleDto> UpdateAsync(
        int scheduleSlotId,
        UpdateScheduleDto request,
        CancellationToken cancellationToken = default);

    // 7. Kích hoạt / vô hiệu hóa Schedule Slot
    Task<DoctorScheduleDto> UpdateStatusAsync(
        int scheduleSlotId,
        bool isActive,
        CancellationToken cancellationToken = default);
}
