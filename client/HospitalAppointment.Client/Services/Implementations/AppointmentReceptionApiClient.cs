using HospitalAppointment.Client.Core;
using HospitalAppointment.Client.Models.Reception;
using HospitalAppointment.Client.Services.Interfaces;

namespace HospitalAppointment.Client.Services.Implementations;

public sealed class AppointmentReceptionApiClient : IAppointmentReceptionApiClient
{
    private readonly ApiClient _api;

    public AppointmentReceptionApiClient(ApiClient api)
    {
        _api = api;
    }

    public Task<ApiResult<List<ReceptionAppointmentModel>>> SearchAppointmentsAsync(
        string? search = null,
        DateOnly? date = null)
    {
        var query = new List<string>();
        if (!string.IsNullOrWhiteSpace(search))
        {
            query.Add($"search={Uri.EscapeDataString(search.Trim())}");
        }

        if (date.HasValue)
        {
            query.Add($"date={date.Value:yyyy-MM-dd}");
        }

        var url = "api/reception/appointments";
        if (query.Count > 0)
        {
            url += "?" + string.Join("&", query);
        }

        return _api.GetAsync<List<ReceptionAppointmentModel>>(url);
    }

    public Task<ApiResult<ReceptionAppointmentModel>> CheckInAppointmentAsync(int appointmentId)
        => _api.PostAsync<object, ReceptionAppointmentModel>(
            $"api/reception/appointments/{appointmentId}/check-in",
            new { });

    public Task<ApiResult<ReceptionAppointmentModel>> MarkNoShowAsync(int appointmentId, string reason)
        => _api.PostAsync<MarkNoShowModel, ReceptionAppointmentModel>(
            $"api/reception/appointments/{appointmentId}/no-show",
            new MarkNoShowModel { Reason = reason });
}
