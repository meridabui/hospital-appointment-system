using HospitalAppointment.Api.DTOs.Confirmation;

namespace HospitalAppointment.Api.Services;

public interface IAppointmentConfirmationService
{
    Task<IReadOnlyList<ConfirmationAppointmentDto>> GetPendingAsync(
        DateOnly? date,
        CancellationToken cancellationToken);

    Task<ConfirmationOperationResult> ConfirmAsync(
        int appointmentId,
        int receptionistUserId,
        CancellationToken cancellationToken);

    Task<ConfirmationOperationResult> RejectAsync(
        int appointmentId,
        int receptionistUserId,
        string reason,
        CancellationToken cancellationToken);
}
