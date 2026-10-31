using HospitalAppointment.Api.Common.Exceptions;
using HospitalAppointment.Api.DTOs.Examination;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using HospitalAppointment.Api.Repositories;
using HospitalAppointment.Api.Repositories.Interfaces;

// Trong Repositories có 2 IAppointmentRepository (đặt lịch và tiếp nhận).
// Luồng khám dùng bản Interfaces (có FindByIdAsync, GetByDoctorStatusesAndDateAsync).
using IAppointmentRepository = HospitalAppointment.Api.Repositories.Interfaces.IAppointmentRepository;

namespace HospitalAppointment.Api.Services;

public class ExaminationService : IExaminationService
{
    private const int HospitalUtcOffsetHours = 7;

    private static readonly AppointmentStatus[] VisibleStatuses =
    {
        AppointmentStatus.ReadyForConsultation,
        AppointmentStatus.InConsultation
    };

    private readonly IAppointmentRepository _appointmentRepository;
    private readonly IDoctorRepository _doctorRepository;
    private readonly IAuditLogRepository _auditLogRepository;

    public ExaminationService(
        IAppointmentRepository appointmentRepository,
        IDoctorRepository doctorRepository,
        IAuditLogRepository auditLogRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
        _auditLogRepository = auditLogRepository;
    }

    public async Task<List<ReadyPatientDto>> GetReadyPatientsAsync(int userId)
    {
        var doctor = await GetActiveDoctorAsync(userId);

        var today = DateOnly.FromDateTime(DateTime.UtcNow.AddHours(HospitalUtcOffsetHours));

        var appointments = await _appointmentRepository.GetByDoctorStatusesAndDateAsync(
            doctor.DoctorId, VisibleStatuses, today);

        return appointments.Select(x => new ReadyPatientDto
        {
            AppointmentId = x.AppointmentId,
            AppointmentCode = x.AppointmentCode,
            Status = x.Status.ToString(),
            BookingReason = x.BookingReason,
            ReadyAt = x.ReadyAt,
            SlotDate = x.ScheduleSlot.SlotDate,
            StartTime = x.ScheduleSlot.StartTime,
            EndTime = x.ScheduleSlot.EndTime,
            DepartmentId = x.DepartmentId,
            DepartmentName = x.Department.DepartmentName,
            PatientId = x.PatientId,
            PatientCode = x.Patient.PatientCode,
            PatientName = x.Patient.FullName,
            Phone = x.Patient.Phone,
            DateOfBirth = x.Patient.DateOfBirth,
            Gender = x.Patient.Gender,
            Address = x.Patient.Address
        }).ToList();
    }

    public async Task StartConsultationAsync(int appointmentId, int userId)
    {
        var doctor = await GetActiveDoctorAsync(userId);

        var appointment = await _appointmentRepository.FindByIdAsync(appointmentId)
            ?? throw new AppException("Không tìm thấy lịch hẹn.", StatusCodes.Status404NotFound);

        if (appointment.DoctorId != doctor.DoctorId)
        {
            throw new AppException("Lịch hẹn này không được phân công cho bạn.", StatusCodes.Status403Forbidden);
        }

        // Ném InvalidOperationException nếu trạng thái không phải ReadyForConsultation
        appointment.StartConsultation();

        // Cùng DbContext (scoped) với appointment nên được lưu chung một lần
        _auditLogRepository.Add(new AuditLog
        {
            UserId = userId,
            Action = "StartConsultation",
            EntityName = "Appointment",
            EntityId = appointment.AppointmentId.ToString(),
            Details = $"Appointment {appointment.AppointmentCode} consultation started",
            CreatedAt = DateTime.UtcNow
        });

        await _appointmentRepository.SaveChangesAsync();
    }

    private async Task<Doctor> GetActiveDoctorAsync(int userId)
    {
        return await _doctorRepository.GetActiveByUserIdAsync(userId)
            ?? throw new AppException("Tài khoản của bạn không phải bác sĩ đang hoạt động.", StatusCodes.Status403Forbidden);
    }
}