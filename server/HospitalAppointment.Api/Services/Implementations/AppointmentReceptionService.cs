using HospitalAppointment.Api.Models.DTOs.Reception;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using HospitalAppointment.Api.Repositories.Interfaces;
using HospitalAppointment.Api.Services.Interfaces;

namespace HospitalAppointment.Api.Services.Implementations;

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

        return appointments.Select(a => new ReceptionAppointmentDto
        {
            AppointmentId = a.AppointmentId,
            AppointmentCode = a.AppointmentCode,
            PatientId = a.PatientId,
            PatientCode = a.Patient.PatientCode,
            PatientName = a.Patient.FullName,
            PatientPhone = a.Patient.Phone,
            PatientDateOfBirth = a.Patient.DateOfBirth,
            PatientGender = a.Patient.Gender,
            DoctorId = a.DoctorId,
            DoctorName = a.Doctor.Staff?.User?.FullName ?? "Bác sĩ chưa cập nhật",
            Specialty = a.Doctor.Specialty,
            DepartmentName = a.Department?.DepartmentName ?? "Chưa phân khoa",
            SlotDate = a.ScheduleSlot.SlotDate,
            StartTime = a.ScheduleSlot.StartTime,
            EndTime = a.ScheduleSlot.EndTime,
            Status = a.Status,
            BookingReason = a.BookingReason,
            CheckedInAt = a.CheckedInAt,
            CheckedInByName = a.CheckedInByUser?.FullName,
            NoShowAt = a.NoShowAt,
            NoShowReason = a.NoShowReason
        }).ToList();
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
        appointment.CheckIn(userId);

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

        if (!appointment.CanMarkAsNoShow())
        {
            throw new InvalidOperationException($"Không thể đánh dấu vắng mặt cho lịch hẹn #{appointment.AppointmentCode} vì đang ở trạng thái '{appointment.Status}'.");
        }

        // Gọi phương thức Domain của Entity theo đúng Class Diagram 3.7.1.1
        appointment.MarkAsNoShow(userId, reason);

        await _appointmentRepository.UpdateAsync(appointment);
        return await _appointmentRepository.SaveChangesAsync();
    }
}

