using HospitalAppointment.Client.Core;
using HospitalAppointment.Client.Models.Confirmation;

namespace HospitalAppointment.Client.Services;

public sealed class AppointmentConfirmationApiClient : IAppointmentConfirmationApiClient
{
    private readonly ApiClient _api;

    public AppointmentConfirmationApiClient(ApiClient api)
    {
        _api = api;
    }

    public Task<ApiResult<List<ConfirmationAppointmentModel>>> GetPendingAsync(DateOnly? date = null)
    {
        var url = "api/appointment-confirmations/pending";
        if (date.HasValue)
        {
            url += $"?date={date.Value:yyyy-MM-dd}";
        }

        return _api.GetAsync<List<ConfirmationAppointmentModel>>(url);
    }

    public Task<ApiResult<ConfirmationAppointmentModel>> ConfirmAsync(int appointmentId)
        => _api.PostAsync<ConfirmationAppointmentModel>(
            $"api/appointment-confirmations/{appointmentId}/confirm");

    public Task<ApiResult<ConfirmationAppointmentModel>> RejectAsync(int appointmentId, string reason)
        => _api.PostAsync<RejectAppointmentModel, ConfirmationAppointmentModel>(
            $"api/appointment-confirmations/{appointmentId}/reject",
            new RejectAppointmentModel { Reason = reason });
}
