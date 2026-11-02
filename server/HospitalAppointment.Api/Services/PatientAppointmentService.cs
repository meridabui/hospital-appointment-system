using HospitalAppointment.Api.Common.Exceptions;
using HospitalAppointment.Api.Common.Helpers;
using HospitalAppointment.Api.DTOs.Patients;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using HospitalAppointment.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Services;

public interface IPatientAppointmentService
{
    Task<List<MyAppointmentDto>> GetMyAppointmentsAsync(int userId);
    Task<CancelAppointmentResponse> CancelAsync(int userId, int appointmentId, CancelAppointmentRequest request);
}

public class PatientAppointmentService : IPatientAppointmentService
{
    // BR-19: chỉ được hủy trước giờ khám ít nhất 24 giờ
    private static readonly TimeSpan MinCancelNotice = TimeSpan.FromHours(24);

    private const string TooLateMessage = "Chỉ có thể hủy lịch hẹn trước giờ khám ít nhất 24 giờ.";

    private readonly IPatientRepository _patients;
    private readonly IAppointmentRepository _appointments;
    private readonly INotificationService _notifications;

    public PatientAppointmentService(IPatientRepository patients, IAppointmentRepository appointments, INotificationService notifications)
    {
        _patients = patients;
        _appointments = appointments;
        _notifications = notifications;
    }

    public async Task<List<MyAppointmentDto>> GetMyAppointmentsAsync(int userId)
    {
        var patient = await GetPatientAsync(userId);
        var appointments = await _appointments.GetByPatientAsync(patient.PatientId);
        var now = ClockHelper.VietnamNow;

        return appointments.Select(a => ToDto(a, now)).ToList();
    }

    public async Task<CancelAppointmentResponse> CancelAsync(int userId, int appointmentId, CancelAppointmentRequest request)
    {
        var patient = await GetPatientAsync(userId);

        // Lọc theo PatientId ngay trong truy vấn: lịch của người khác sẽ trả về "không tìm thấy" (BR-10)
        var appointment = await _appointments.GetOwnedForUpdateAsync(appointmentId, patient.PatientId)
                          ?? throw new AppException("Không tìm thấy lịch hẹn.", StatusCodes.Status404NotFound);

        if (appointment.Status is not (AppointmentStatus.Pending or AppointmentStatus.Confirmed))
            throw new AppException(
                $"Không thể hủy lịch hẹn đang ở trạng thái {appointment.Status}.",
                StatusCodes.Status409Conflict);

        var startsAt = appointment.ScheduleSlot.SlotDate.ToDateTime(appointment.ScheduleSlot.StartTime);
        if (startsAt - ClockHelper.VietnamNow < MinCancelNotice)
            throw new AppException(TooLateMessage); // MSG12

        var cancelledAt = DateTime.UtcNow;
        appointment.Cancel(userId, request.Reason, cancelledAt);

        _appointments.AddAuditLog(new AuditLog
        {
            UserId = userId,
            Action = "CancelAppointment",
            EntityName = "Appointment",
            EntityId = appointment.AppointmentCode,
            Details = string.IsNullOrWhiteSpace(request.Reason)
                ? "Patient cancelled the appointment."
                : $"Patient cancelled the appointment. Reason: {request.Reason.Trim()}",
            CreatedAt = cancelledAt
        });

        try
        {
            await _appointments.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            // Lễ tân vừa xác nhận/từ chối cùng lúc (RowVersion đã đổi)
            throw new AppException(
                "Lịch hẹn vừa được cập nhật bởi người khác. Vui lòng tải lại danh sách rồi thử lại.",
                StatusCodes.Status409Conflict);
        }
        await _notifications.SendAppointmentCancelledAsync(appointment);

        return new CancelAppointmentResponse
        {
            AppointmentId = appointment.AppointmentId,
            AppointmentCode = appointment.AppointmentCode,
            Status = appointment.Status.ToString(),
            CancelledAt = cancelledAt,
            Message = "Hủy lịch hẹn thành công." // MSG06
        };
    }

    private async Task<Patient> GetPatientAsync(int userId)
        => await _patients.GetActiveByUserIdAsync(userId)
           ?? throw new AppException("Tài khoản không có quyền thực hiện thao tác này.", StatusCodes.Status403Forbidden);

    private static MyAppointmentDto ToDto(Appointment a, DateTime now)
    {
        var startsAt = a.ScheduleSlot.SlotDate.ToDateTime(a.ScheduleSlot.StartTime);
        var cancellableStatus = a.Status is AppointmentStatus.Pending or AppointmentStatus.Confirmed;
        var enoughNotice = startsAt - now >= MinCancelNotice;

        return new MyAppointmentDto
        {
            AppointmentId = a.AppointmentId,
            AppointmentCode = a.AppointmentCode,
            Status = a.Status.ToString(),
            DepartmentName = a.Department.DepartmentName,
            DoctorName = a.Doctor.Staff.User.FullName,
            Date = a.ScheduleSlot.SlotDate,
            StartTime = a.ScheduleSlot.StartTime,
            EndTime = a.ScheduleSlot.EndTime,
            BookingReason = a.BookingReason,
            RejectionReason = a.RejectionReason,
            CancellationReason = a.CancellationReason,
            CancelledAt = a.CancelledAt,
            CreatedAt = a.CreatedAt,
            CanCancel = cancellableStatus && enoughNotice,
            CancelBlockedReason = cancellableStatus && !enoughNotice ? TooLateMessage : null,
            HasMedicalRecord = a.MedicalRecord is { RecordStatus: RecordStatus.Final }
        };
    }
}
