using System.Net.Http.Json;
using System.Text.Json;
using HospitalAppointment.Client.Models.Reception;
using HospitalAppointment.Client.Services.Interfaces;

namespace HospitalAppointment.Client.Services.Implementations;

public class AppointmentReceptionApiClient : IAppointmentReceptionApiClient
{
    private readonly HttpClient _http;
    private readonly string _apiBaseUrl = "http://localhost:5005/api/reception";

    public AppointmentReceptionApiClient(HttpClient http, IConfiguration configuration)
    {
        _http = http;
        var configuredUrl = configuration["ApiBaseUrl"];
        if (!string.IsNullOrWhiteSpace(configuredUrl))
        {
            _apiBaseUrl = $"{configuredUrl.TrimEnd('/')}/api/reception";
        }
    }

    public async Task<List<ReceptionAppointmentModel>> SearchAppointmentsAsync(string? keyword = null, DateOnly? date = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (!string.IsNullOrWhiteSpace(keyword))
            {
                queryParams.Add($"keyword={Uri.EscapeDataString(keyword.Trim())}");
            }
            if (date.HasValue)
            {
                queryParams.Add($"date={date.Value:yyyy-MM-dd}");
            }

            var url = $"{_apiBaseUrl}/appointments";
            if (queryParams.Count > 0)
            {
                url += "?" + string.Join("&", queryParams);
            }

            var result = await _http.GetFromJsonAsync<List<ReceptionAppointmentModel>>(url);
            return result ?? new List<ReceptionAppointmentModel>();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[ApiClient] Error searching appointments: {ex.Message}");
            return new List<ReceptionAppointmentModel>();
        }
    }

    public async Task<(bool Success, string Message)> CheckInAppointmentAsync(int appointmentId, int? userId = null)
    {
        try
        {
            var url = $"{_apiBaseUrl}/appointments/{appointmentId}/check-in";
            var response = await _http.PostAsJsonAsync(url, new { UserId = userId ?? 4 });

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<JsonElement>();
                var message = content.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Check-in thành công.";
                return (true, message ?? "Check-in thành công.");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    if (errObj.TryGetProperty("message", out var msgProp))
                    {
                        return (false, msgProp.GetString() ?? "Lỗi xử lý check-in.");
                    }
                }
                catch { }
                return (false, "Lỗi từ máy chủ: " + response.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            return (false, "Không thể kết nối máy chủ API: " + ex.Message);
        }
    }

    public async Task<(bool Success, string Message)> MarkNoShowAsync(int appointmentId, string reason, int? userId = null)
    {
        try
        {
            var url = $"{_apiBaseUrl}/appointments/{appointmentId}/no-show";
            var response = await _http.PostAsJsonAsync(url, new { UserId = userId ?? 4, Reason = reason });

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadFromJsonAsync<JsonElement>();
                var message = content.TryGetProperty("message", out var msgProp) ? msgProp.GetString() : "Đã báo vắng thành công.";
                return (true, message ?? "Đã báo vắng thành công.");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                try
                {
                    var errObj = JsonSerializer.Deserialize<JsonElement>(errorContent);
                    if (errObj.TryGetProperty("message", out var msgProp))
                    {
                        return (false, msgProp.GetString() ?? "Lỗi xử lý No-show.");
                    }
                }
                catch { }
                return (false, "Lỗi từ máy chủ: " + response.ReasonPhrase);
            }
        }
        catch (Exception ex)
        {
            return (false, "Không thể kết nối máy chủ API: " + ex.Message);
        }
    }
}

