using HospitalAppointment.Client.Api.Models;
using HospitalAppointment.Client.Core;

namespace HospitalAppointment.Client.Api;

public class ExaminationApi
{
    private readonly ApiClient _api;

    public ExaminationApi(ApiClient api)
    {
        _api = api;
    }

    public Task<ApiResult<List<ReadyPatient>>> GetReadyPatientsAsync()
    {
        return _api.GetAsync<List<ReadyPatient>>("api/examination/ready-patients");
    }

    public Task<ApiResult<object>> StartConsultationAsync(int appointmentId)
    {
        return _api.PostAsync<object>($"api/examination/{appointmentId}/start");
    }
}
