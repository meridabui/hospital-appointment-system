using HospitalAppointment.Api.DTOs.Nurse;

namespace HospitalAppointment.Api.Services;

public interface INurseService
{
    Task<List<NurseAppointmentDto>> GetCheckedInAppointmentsAsync(
        CancellationToken cancellationToken = default);

    Task<NurseAppointmentDto> GetAppointmentDetailAsync(
        int appointmentId,
        CancellationToken cancellationToken = default);

    Task<NurseAppointmentDto> MarkReadyAsync(
        int appointmentId,
        int nurseUserId,
        CancellationToken cancellationToken = default);
}