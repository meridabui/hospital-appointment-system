using HospitalAppointment.Client.Core;
using HospitalAppointment.Client.Models.Confirmation;

namespace HospitalAppointment.Client.Services;

public interface IAppointmentConfirmationApiClient
{
    Task<ApiResult<List<ConfirmationAppointmentModel>>> GetPendingAsync(DateOnly? date = null);

    Task<ApiResult<ConfirmationAppointmentModel>> ConfirmAsync(int appointmentId);

    Task<ApiResult<ConfirmationAppointmentModel>> RejectAsync(int appointmentId, string reason);
}
