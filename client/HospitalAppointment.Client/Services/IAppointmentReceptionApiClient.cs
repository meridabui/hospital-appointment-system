using HospitalAppointment.Client.Core;
using HospitalAppointment.Client.Models.Reception;

namespace HospitalAppointment.Client.Services;

public interface IAppointmentReceptionApiClient
{
    Task<ApiResult<List<ReceptionAppointmentModel>>> SearchAppointmentsAsync(
        string? search = null,
        DateOnly? date = null);

    Task<ApiResult<ReceptionAppointmentModel>> CheckInAppointmentAsync(int appointmentId);

    Task<ApiResult<ReceptionAppointmentModel>> MarkNoShowAsync(int appointmentId, string reason);
}
