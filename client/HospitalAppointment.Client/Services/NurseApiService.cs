using HospitalAppointment.Client.Models.Nurse;
using System.Net.Http.Json;

namespace HospitalAppointment.Client.Services;

public class NurseApiService
{
    private readonly HttpClient _httpClient;

    public NurseApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // 1. Lấy danh sách bệnh nhân đã Check-in
    public async Task<List<NurseAppointmentDto>>
        GetCheckedInAppointmentsAsync()
    {
        var result = await _httpClient.GetFromJsonAsync<
            List<NurseAppointmentDto>>(
                "api/Nurse/appointments/checked-in");

        return result ?? new List<NurseAppointmentDto>();
    }

    // 2. Lấy chi tiết Appointment
    public async Task<NurseAppointmentDto?>
        GetAppointmentDetailAsync(int appointmentId)
    {
        var response = await _httpClient.GetAsync(
            $"api/Nurse/appointments/{appointmentId}");

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return null;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<
            NurseAppointmentDto>();
    }

    // 3. MarkReady
    public async Task<MarkReadyResponseDto> MarkReadyAsync(
        int appointmentId)
    {
        var response = await _httpClient.PostAsync(
            $"api/Nurse/appointments/{appointmentId}/mark-ready",
            null);

        response.EnsureSuccessStatusCode();

        var result = await response.Content
            .ReadFromJsonAsync<MarkReadyResponseDto>();

        return result ?? throw new InvalidOperationException(
            "Server không trả về kết quả MarkReady.");
    }
}