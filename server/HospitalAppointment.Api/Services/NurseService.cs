using HospitalAppointment.Api.Data.DTOs.Nurse;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using HospitalAppointment.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Services;

public class NurseService : INurseService
{
    private readonly INurseRepository _nurseRepository;

    public NurseService(INurseRepository nurseRepository)
    {
        _nurseRepository = nurseRepository;
    }

    // 1. Danh sách bệnh nhân chờ Nurse
    public async Task<List<NurseAppointmentDto>>
        GetCheckedInAppointmentsAsync(
            CancellationToken cancellationToken = default)
    {
        var appointments =
            await _nurseRepository.GetCheckedInAppointmentsAsync(
                cancellationToken);

        return appointments.Select(MapToDto).ToList();
    }

    // 2. Chi tiết bệnh nhân
    public async Task<NurseAppointmentDto> GetAppointmentDetailAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        if (appointmentId <= 0)
            throw new ArgumentException("Invalid appointment ID.");

        var appointment = await _nurseRepository.GetByIdAsync(
            appointmentId,
            cancellationToken);

        if (appointment == null)
            throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status != AppointmentStatus.CheckedIn)
        {
            throw new InvalidOperationException(
                "Appointment is not waiting for nurse preparation.");
        }

        return MapToDto(appointment);
    }

    // 3. MarkReady
    public async Task<NurseAppointmentDto> MarkReadyAsync(
        int appointmentId,
        int nurseUserId,
        CancellationToken cancellationToken = default)
    {
        if (appointmentId <= 0 || nurseUserId <= 0)
            throw new ArgumentException("Invalid input.");

        var appointment = await _nurseRepository.GetByIdAsync(
            appointmentId,
            cancellationToken);

        if (appointment == null)
            throw new KeyNotFoundException("Appointment not found.");

        if (appointment.Status != AppointmentStatus.CheckedIn)
        {
            throw new InvalidOperationException(
                "Only checked-in appointments can be marked as ready.");
        }

        appointment.Status = AppointmentStatus.ReadyForConsultation;
        appointment.ReadyAt = DateTime.UtcNow;
        //appointment.ReadyBy = nurseUserId;

        try
        {
            await _nurseRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new InvalidOperationException(
                "Appointment was modified by another user. Please refresh.");
        }

        return MapToDto(appointment);
    }

    // Entity -> DTO
    private static NurseAppointmentDto MapToDto(Appointment appointment)
    {
        return new NurseAppointmentDto
        {
            AppointmentId = appointment.AppointmentId,
            AppointmentCode = appointment.AppointmentCode,

            PatientId = appointment.PatientId,
            PatientCode = appointment.Patient.PatientCode,
            PatientName = appointment.Patient.FullName,
            PatientPhone = appointment.Patient.Phone,

            DoctorId = appointment.DoctorId,
            DoctorName = appointment.Doctor.Staff.User.FullName,

            DepartmentId = appointment.DepartmentId,
            DepartmentName = appointment.Department.DepartmentName,

            SlotDate = appointment.ScheduleSlot.SlotDate,
            StartTime = appointment.ScheduleSlot.StartTime,
            EndTime = appointment.ScheduleSlot.EndTime,

            Status = appointment.Status.ToString(),
            CheckedInAt = appointment.CheckedInAt,
            ReadyAt = appointment.ReadyAt
            //ReadyBy = appointment.ReadyBy
        };
    }
}