using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;

namespace HospitalAppointment.Api.Repositories.Interfaces;

public interface IAppointmentRepository
{
    /// <summary>
    /// Tìm kiếm lịch hẹn theo mã hẹn hoặc tên bệnh nhân/SĐT theo Class Diagram 3.7.1.1
    /// </summary>
    Task<List<Appointment>> SearchByCodeOrPatientNameAsync(string? keyword, DateOnly? date = null);

    /// <summary>
    /// Tìm chi tiết lịch hẹn theo ID kèm các entity liên quan theo Class Diagram 3.7.1.1
    /// </summary>
    Task<Appointment?> FindByIdAsync(int id);

    /// <summary>
    /// Danh sách lịch hẹn của bác sĩ theo trạng thái và ngày khám (luồng Examination)
    /// </summary>
    Task<List<Appointment>> GetByDoctorStatusesAndDateAsync(int doctorId, AppointmentStatus[] statuses, DateOnly slotDate);

    /// <summary>
    /// Cập nhật thông tin Appointment theo Class Diagram 3.7.1.1
    /// </summary>
    Task UpdateAsync(Appointment appointment);

    /// <summary>
    /// Lưu các thay đổi xuống cơ sở dữ liệu
    /// </summary>
    Task<bool> SaveChangesAsync();
}
