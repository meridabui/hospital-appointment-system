using HospitalAppointment.Api.Models.DTOs.Reception;
using HospitalAppointment.Api.Models.Entities;

namespace HospitalAppointment.Api.Services.Interfaces;

public interface IAppointmentReceptionService
{
    /// <summary>
    /// Tra cứu danh sách lịch hẹn phục vụ tiếp nhận tại quầy theo Class Diagram 3.7.1.1
    /// </summary>
    Task<List<ReceptionAppointmentDto>> SearchAppointmentsAsync(string? keyword, DateOnly? date = null);

    /// <summary>
    /// Kiểm tra tính hợp lệ của lịch hẹn trước khi check-in theo Class Diagram 3.7.1.1
    /// </summary>
    bool ValidateCheckInEligibility(Appointment appointment);

    /// <summary>
    /// Thực hiện check-in cho lịch hẹn theo Class Diagram 3.7.1.1 & Sequence Diagram 3.7.2.2
    /// </summary>
    Task<bool> CheckInAppointmentAsync(int appointmentId, int userId);

    /// <summary>
    /// Đánh dấu bệnh nhân vắng mặt (No-show) theo Class Diagram 3.7.1.1 & Sequence Diagram 3.7.2.3
    /// </summary>
    Task<bool> MarkNoShowAsync(int id, int userId, string reason);
}

