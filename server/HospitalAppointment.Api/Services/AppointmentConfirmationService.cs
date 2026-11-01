using HospitalAppointment.Api.DTOs.Confirmation;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using HospitalAppointment.Api.Repositories;

namespace HospitalAppointment.Api.Services;

public sealed class AppointmentConfirmationService : IAppointmentConfirmationService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IHospitalClock _clock;

    public AppointmentConfirmationService(IUnitOfWork unitOfWork, IHospitalClock clock)
    {
        _unitOfWork = unitOfWork;
        _clock = clock;
    }

    public async Task<IReadOnlyList<ConfirmationAppointmentDto>> GetPendingAsync(
        DateOnly? date,
        CancellationToken cancellationToken)
    {
        var appointments = await _unitOfWork.Appointments.SearchForReceptionAsync(
            null, date, AppointmentStatus.Pending, cancellationToken);

        return appointments.Select(Map).ToList();
    }

    public async Task<ConfirmationOperationResult> ConfirmAsync(
        int appointmentId,
        int receptionistUserId,
        CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetForReceptionUpdateAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return ConfirmationOperationResult.Failed(ConfirmationFailure.NotFound, "Appointment not found.");
        }

        if (appointment.Status != AppointmentStatus.Pending)
        {
            return ConfirmationOperationResult.Failed(
                ConfirmationFailure.InvalidState,
                "Only a pending appointment can be confirmed.");
        }

        appointment.Confirm(receptionistUserId, _clock.UtcNow);
        AddAuditLog(appointment, receptionistUserId, "AppointmentConfirmed", "Appointment confirmed by reception.");
        await _unitOfWork.SaveChangesAsync();

        return ConfirmationOperationResult.Success(Map(appointment));
    }

    public async Task<ConfirmationOperationResult> RejectAsync(
        int appointmentId,
        int receptionistUserId,
        string reason,
        CancellationToken cancellationToken)
    {
        var appointment = await _unitOfWork.Appointments.GetForReceptionUpdateAsync(appointmentId, cancellationToken);
        if (appointment is null)
        {
            return ConfirmationOperationResult.Failed(ConfirmationFailure.NotFound, "Appointment not found.");
        }

        if (appointment.Status != AppointmentStatus.Pending)
        {
            return ConfirmationOperationResult.Failed(
                ConfirmationFailure.InvalidState,
                "Only a pending appointment can be rejected.");
        }

        appointment.Reject(reason);
        AddAuditLog(appointment, receptionistUserId, "AppointmentRejected", $"Appointment rejected. Reason: {reason.Trim()}");
        await _unitOfWork.SaveChangesAsync();

        return ConfirmationOperationResult.Success(Map(appointment));
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

    private static ConfirmationAppointmentDto Map(Appointment appointment) => new(
        appointment.AppointmentId,
        appointment.AppointmentCode,
        appointment.Status.ToString(),
        appointment.Patient.FullName,
        appointment.Patient.Phone,
        appointment.Doctor.Staff.User.FullName,
        appointment.Department.DepartmentName,
        appointment.ScheduleSlot.SlotDate,
        appointment.ScheduleSlot.StartTime,
        appointment.ScheduleSlot.EndTime,
        appointment.BookingReason,
        appointment.ConfirmedAt,
        appointment.RejectionReason);
}
