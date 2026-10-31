using HospitalAppointment.Api.DTOs.Reception;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using HospitalAppointment.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Services;

public sealed class ReceptionService : IReceptionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHospitalClock _clock;

    public ReceptionService(
        IUnitOfWork unitOfWork,
        IHospitalClock clock)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<IReadOnlyList<ReceptionAppointmentDto>> SearchAsync(
        string? search,
        DateOnly? date,
        AppointmentStatus? status,
        CancellationToken cancellationToken)
    {
        var appointments = await _unitOfWork.Appointments.SearchForReceptionAsync(
            search,
            date,
            status,
            cancellationToken);

        return appointments.Select(Map).ToList();
    }

    public async Task<ReceptionOperationResult> CheckInAsync(
        int appointmentId,
        int receptionistUserId,
        CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetForReceptionUpdateAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return ReceptionOperationResult.Failed(ReceptionFailure.NotFound, "Appointment not found.");
        }

        if (appointment.Status != AppointmentStatus.Confirmed)
        {
            return ReceptionOperationResult.Failed(
                ReceptionFailure.InvalidState,
                "Only a confirmed appointment can be checked in.");
        }

        appointment.CheckIn(receptionistUserId, _clock.UtcNow);
        AddAuditLog(appointment, receptionistUserId, "AppointmentCheckIn", "Patient checked in at reception.");

        return await SaveAsync(appointment, cancellationToken);
    }

    public async Task<ReceptionOperationResult> MarkNoShowAsync(
        int appointmentId,
        int receptionistUserId,
        string reason,
        CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetForReceptionUpdateAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return ReceptionOperationResult.Failed(ReceptionFailure.NotFound, "Appointment not found.");
        }

        if (appointment.Status != AppointmentStatus.Confirmed)
        {
            return ReceptionOperationResult.Failed(
                ReceptionFailure.InvalidState,
                "Only a confirmed appointment can be marked as no-show.");
        }

        var scheduledAt = appointment.ScheduleSlot.SlotDate.ToDateTime(appointment.ScheduleSlot.StartTime);
        if (_clock.HospitalNow < scheduledAt)
        {
            return ReceptionOperationResult.Failed(
                ReceptionFailure.TooEarlyForNoShow,
                "An appointment cannot be marked as no-show before its scheduled time.");
        }

        appointment.MarkNoShow(receptionistUserId, _clock.UtcNow, reason);
        AddAuditLog(appointment, receptionistUserId, "AppointmentNoShow", $"No-show reason: {reason.Trim()}");

        return await SaveAsync(appointment, cancellationToken);
    }

    private async Task<ReceptionOperationResult> SaveAsync(
        Appointment appointment,
        CancellationToken cancellationToken)
    {
        try
        {
            await _unitOfWork.SaveChangesAsync();
            return ReceptionOperationResult.Success(Map(appointment));
        }
        catch (DbUpdateConcurrencyException)
        {
            return ReceptionOperationResult.Failed(
                ReceptionFailure.ConcurrencyConflict,
                "This appointment was changed by another user. Refresh the list and try again.");
        }
    }

    private void AddAuditLog(Appointment appointment, int userId, string action, string details)
    {
        _unitOfWork.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = nameof(Appointment),
            EntityId = appointment.AppointmentId.ToString(),
            Details = details,
            CreatedAt = _clock.UtcNow
        });
    }

    private static ReceptionAppointmentDto Map(Appointment appointment)
    {
        return new ReceptionAppointmentDto(
            appointment.AppointmentId,
            appointment.AppointmentCode,
            appointment.Status.ToString(),
            appointment.PatientId,
            appointment.Patient.PatientCode,
            appointment.Patient.FullName,
            appointment.Patient.Phone,
            appointment.Patient.DateOfBirth,
            appointment.Patient.Gender,
            appointment.Doctor.Staff.User.FullName,
            appointment.Doctor.Specialty,
            appointment.Department.DepartmentName,
            appointment.ScheduleSlot.SlotDate,
            appointment.ScheduleSlot.StartTime,
            appointment.ScheduleSlot.EndTime,
            appointment.BookingReason,
            appointment.CheckedInAt,
            appointment.NoShowAt,
            appointment.NoShowReason);
    }
}
