using HospitalAppointment.Api.DTOs.Reception;
using HospitalAppointment.Api.Models.Enums;

namespace HospitalAppointment.Api.Services.Reception;

public interface IReceptionService
{
    Task<IReadOnlyList<ReceptionAppointmentDto>> SearchAsync(
        string? search,
        DateOnly? date,
        AppointmentStatus? status,
        CancellationToken cancellationToken);

    Task<ReceptionOperationResult> CheckInAsync(
        int appointmentId,
        int receptionistUserId,
        CancellationToken cancellationToken);

    Task<ReceptionOperationResult> MarkNoShowAsync(
        int appointmentId,
        int receptionistUserId,
        string reason,
        CancellationToken cancellationToken);
}
