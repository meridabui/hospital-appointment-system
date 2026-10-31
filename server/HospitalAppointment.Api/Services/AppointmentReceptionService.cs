using HospitalAppointment.Api.DTOs.Reception;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using HospitalAppointment.Api.Repositories.Interfaces;
namespace HospitalAppointment.Api.Services;

public class AppointmentReceptionService : IAppointmentReceptionService
{
    private readonly IAppointmentRepository _appointmentRepository;

    public AppointmentReceptionService(IAppointmentRepository appointmentRepository)
    {
        _appointmentRepository = appointmentRepository;
    }
    public async Task<List<ReceptionAppointmentDto>> SearchAppointmentsAsync(string? keyword, DateOnly? date = null)
    {
        var appointments = await _appointmentRepository.SearchByCodeOrPatientNameAsync(keyword, date);

        return appointments.Select(a => new ReceptionAppointmentDto(
       a.AppointmentId,
       a.AppointmentCode,
       a.Status.ToString(),
       a.PatientId,
       a.Patient.PatientCode,
       a.Patient.FullName,
       a.Patient.Phone,
       a.Patient.DateOfBirth,
       a.Patient.Gender,
       a.Doctor.Staff?.User?.FullName ?? "Bác sĩ chưa cập nhật",
       a.Doctor.Specialty,
       a.Department?.DepartmentName ?? "Chưa phân khoa",
       a.ScheduleSlot.SlotDate,
       a.ScheduleSlot.StartTime,
       a.ScheduleSlot.EndTime,
       a.BookingReason,
       a.CheckedInAt,
       a.NoShowAt,
       a.NoShowReason)).ToList();
    }

    public bool ValidateCheckInEligibility(Appointment appointment)
    {
        // Theo nghiệp vụ SRS & SDS: Chỉ lịch hẹn ở trạng thái Confirmed mới đủ điều kiện check-in
        return appointment.Status == AppointmentStatus.Confirmed;
    }

    public async Task<bool> CheckInAppointmentAsync(int appointmentId, int userId)
    {
        var appointment = await _appointmentRepository.FindByIdAsync(appointmentId);
        if (appointment == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lịch hẹn với mã ID {appointmentId}.");
        }

        if (!ValidateCheckInEligibility(appointment))
        {
            throw new InvalidOperationException($"Lịch hẹn #{appointment.AppointmentCode} không thể check-in vì đang ở trạng thái '{appointment.Status}'. Chỉ lịch hẹn 'Confirmed' mới được phép check-in.");
        }

        // Gọi phương thức Domain của Entity theo đúng Class Diagram 3.7.1.1
        appointment.CheckIn(userId, DateTime.UtcNow);
        await _appointmentRepository.UpdateAsync(appointment);
        return await _appointmentRepository.SaveChangesAsync();
    }

    public async Task<bool> MarkNoShowAsync(int id, int userId, string reason)
    {
        var appointment = await _appointmentRepository.FindByIdAsync(id);
        if (appointment == null)
        {
            throw new KeyNotFoundException($"Không tìm thấy lịch hẹn với mã ID {id}.");
        }

        if (appointment.Status != AppointmentStatus.Confirmed)
        {
            throw new InvalidOperationException($"Không thể đánh dấu vắng mặt cho lịch hẹn #{appointment.AppointmentCode} vì đang ở trạng thái '{appointment.Status}'.");
        }

        // Gọi phương thức Domain của Entity theo đúng Class Diagram 3.7.1.1
        appointment.MarkNoShow(userId, DateTime.UtcNow, reason);

        await _appointmentRepository.UpdateAsync(appointment);
        return await _appointmentRepository.SaveChangesAsync();
    }
}

