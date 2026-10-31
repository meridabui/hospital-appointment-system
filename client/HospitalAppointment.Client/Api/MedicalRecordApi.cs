using HospitalAppointment.Client.Api.Models;
using HospitalAppointment.Client.Core;

namespace HospitalAppointment.Client.Api;

public class MedicalRecordApi
{
    private readonly ApiClient _api;

    public MedicalRecordApi(ApiClient api)
    {
        _api = api;
    }

    public Task<ApiResult<MedicalRecordDetail>> GetAsync(int appointmentId)
    {
        return _api.GetAsync<MedicalRecordDetail>($"api/medical-records/{appointmentId}");
    }

    public Task<ApiResult<MedicalRecordDetail>> SaveAsync(int appointmentId, SaveMedicalRecordForm form)
    {
        return _api.PutAsync<SaveMedicalRecordForm, MedicalRecordDetail>($"api/medical-records/{appointmentId}", form);
    }

    public Task<ApiResult<MedicalRecordDetail>> CompleteAsync(int appointmentId)
    {
        return _api.PostAsync<MedicalRecordDetail>($"api/medical-records/{appointmentId}/complete");
    }

    public Task<ApiResult<List<CompletedVisit>>> GetCompletedAsync(string? keyword, DateOnly? date)
    {
        var url = $"api/medical-records/completed?keyword={Uri.EscapeDataString(keyword ?? string.Empty)}";
        if (date.HasValue)
        {
            url += $"&date={date.Value:yyyy-MM-dd}";
        }

        return _api.GetAsync<List<CompletedVisit>>(url);
    }

    public Task<ApiResult<List<ActiveMedicalService>>> GetActiveServicesAsync()
    {
        return _api.GetAsync<List<ActiveMedicalService>>("api/medical-records/services");
    }
}
