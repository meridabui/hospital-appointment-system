using HospitalAppointment.Client.Api.Models;
using HospitalAppointment.Client.Core;

namespace HospitalAppointment.Client.Api;

public class ExaminationApi
{
    private readonly ApiHttpClient _httpClient;

    public ExaminationApi(ApiHttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // TODO: remove userId when Auth is merged. The API will read it from the JWT.
    public Task<List<ReadyPatient>> GetReadyPatientsAsync(int userId)
    {
        return _httpClient.GetAsync<List<ReadyPatient>>($"api/examination/ready-patients?userId={userId}");
    }

    public Task StartConsultationAsync(int appointmentId, int userId)
    {
        return _httpClient.PostAsync($"api/examination/{appointmentId}/start?userId={userId}");
    }
}
