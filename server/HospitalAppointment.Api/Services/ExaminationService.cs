using HospitalAppointment.Api.DTOs.Examination;
using HospitalAppointment.Api.Exceptions;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using HospitalAppointment.Api.Repositories;

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

    public ExaminationService(IAppointmentRepository appointmentRepository, IDoctorRepository doctorRepository)
    {
        _appointmentRepository = appointmentRepository;
        _doctorRepository = doctorRepository;
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

        var appointment = await _appointmentRepository.GetByIdAsync(appointmentId)
            ?? throw new NotFoundException("Appointment not found.");

        if (appointment.DoctorId != doctor.DoctorId)
        {
            throw new UnauthorizedAccessException("This appointment is not assigned to you.");
        }

        appointment.StartConsultation();

        _appointmentRepository.AddAuditLog(new AuditLog
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
            ?? throw new UnauthorizedAccessException("Your account is not an active doctor.");
    }
}
