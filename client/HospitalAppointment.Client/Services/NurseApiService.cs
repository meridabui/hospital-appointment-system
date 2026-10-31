
using HospitalAppointment.Client.Models.Nurse;
using System.Net;
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
        var response = await _httpClient.GetAsync(
            "api/Nurse/appointments/checked-in");

        await EnsureSuccessAsync(response);

        return await response.Content.ReadFromJsonAsync<
            List<NurseAppointmentDto>>() ?? new List<NurseAppointmentDto>();
    }

    // 2. Lấy chi tiết Appointment
    public async Task<NurseAppointmentDto>
        GetAppointmentDetailAsync(int appointmentId)
    {
        var response = await _httpClient.GetAsync(
            $"api/Nurse/appointments/{appointmentId}");

        await EnsureSuccessAsync(response);

        var result = await response.Content.ReadFromJsonAsync<
            NurseAppointmentDto>();

        return result ?? throw new InvalidOperationException(
            "The server returned an empty appointment.");
    }

    // 3. Xác nhận bệnh nhân sẵn sàng khám
    public async Task<MarkReadyResponseDto> MarkReadyAsync(
        int appointmentId)
    {
        var response = await _httpClient.PostAsync(
            $"api/Nurse/appointments/{appointmentId}/mark-ready",
            null);

        await EnsureSuccessAsync(response);

        var result = await response.Content.ReadFromJsonAsync<
            MarkReadyResponseDto>();

        return result ?? throw new InvalidOperationException(
            "The server returned an empty MarkReady response.");
    }

    // Xử lý lỗi HTTP
    private static async Task EnsureSuccessAsync(
        HttpResponseMessage response)
    {
        if (response.IsSuccessStatusCode)
            return;

        var message = response.StatusCode switch
        {
            HttpStatusCode.Unauthorized =>
                "Your session has expired. Please log in again.",

            HttpStatusCode.Forbidden =>
                "You do not have permission to perform this action.",

            HttpStatusCode.NotFound =>
                "The requested appointment was not found.",

            HttpStatusCode.Conflict =>
                "The appointment status has changed. Please refresh.",

            _ => "An error occurred while communicating with the server."
        };

        throw new HttpRequestException(
            message,
            null,
            response.StatusCode);
    }
}
