
using HospitalAppointment.Api.Models.Entities;

namespace HospitalAppointment.Api.Repositories;

public interface IScheduleRepository
{
    // 1. Lấy danh sách Schedule Slot, hỗ trợ lọc
    Task<List<DoctorScheduleSlot>> GetAllAsync(
        int? doctorId = null,
        int? departmentId = null,
        DateOnly? slotDate = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default);

    // 2. Lấy chi tiết Schedule Slot
    Task<DoctorScheduleSlot?> GetByIdAsync(
        int scheduleSlotId,
        CancellationToken cancellationToken = default);

    // 3. Lấy danh sách Doctor đang hoạt động
    Task<List<Doctor>> GetDoctorsAsync(
        CancellationToken cancellationToken = default);

    // 4. Lấy Doctor theo ID
    Task<Doctor?> GetDoctorByIdAsync(
        int doctorId,
        CancellationToken cancellationToken = default);

    // 5. Lấy Department mà Doctor được phân công
    Task<List<Department>> GetDepartmentsByDoctorAsync(
        int doctorId,
        CancellationToken cancellationToken = default);

    // 6. Kiểm tra Doctor có được phân công Department không
    Task<bool> IsDoctorAssignedToDepartmentAsync(
        int doctorId,
        int departmentId,
        CancellationToken cancellationToken = default);

    // 7. Kiểm tra slot có bị trùng hoặc giao nhau không
    Task<bool> HasOverlapAsync(
        int doctorId,
        DateOnly slotDate,
        TimeOnly startTime,
        TimeOnly endTime,
        int? excludeScheduleSlotId = null,
        CancellationToken cancellationToken = default);

    // 8. Kiểm tra slot có Appointment đang chiếm không
    Task<bool> HasActiveAppointmentAsync(
        int scheduleSlotId,
        CancellationToken cancellationToken = default);

    // 9. Thêm nhiều Schedule Slot
    Task AddRangeAsync(
        IEnumerable<DoctorScheduleSlot> scheduleSlots,
        CancellationToken cancellationToken = default);

    // 10. Lưu thay đổi
    Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default);
}
