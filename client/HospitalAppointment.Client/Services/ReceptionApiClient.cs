using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using HospitalAppointment.Client.Models;

namespace HospitalAppointment.Client.Services;

public sealed class ReceptionApiClient
{
    private readonly HttpClient _httpClient;

    public ReceptionApiClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<ApiResult<IReadOnlyList<ReceptionAppointment>>> SearchAsync(
        string? search,
        DateOnly? date,
        string? status,
        CancellationToken cancellationToken = default)
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

        if (!string.IsNullOrWhiteSpace(status))
        {
            query.Add($"status={Uri.EscapeDataString(status)}");
        }

        var path = "api/reception/appointments";
        if (query.Count > 0)
        {
            path += "?" + string.Join("&", query);
        }

        using var response = await _httpClient.GetAsync(path, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<IReadOnlyList<ReceptionAppointment>>.Failed(await ReadErrorAsync(response, cancellationToken));
        }

        var appointments = await response.Content.ReadFromJsonAsync<List<ReceptionAppointment>>(cancellationToken);
        return ApiResult<IReadOnlyList<ReceptionAppointment>>.Success(appointments ?? []);
    }

    public Task<ApiResult<ReceptionAppointment>> CheckInAsync(
        int appointmentId,
        CancellationToken cancellationToken = default)
    {
        return PostAsync($"api/reception/appointments/{appointmentId}/check-in", null, cancellationToken);
    }

    public Task<ApiResult<ReceptionAppointment>> MarkNoShowAsync(
        int appointmentId,
        string reason,
        CancellationToken cancellationToken = default)
    {
        return PostAsync(
            $"api/reception/appointments/{appointmentId}/no-show",
            new { Reason = reason },
            cancellationToken);
    }

    private async Task<ApiResult<ReceptionAppointment>> PostAsync(
        string path,
        object? body,
        CancellationToken cancellationToken)
    {
        using var response = body is null
            ? await _httpClient.PostAsync(path, null, cancellationToken)
            : await _httpClient.PostAsJsonAsync(path, body, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            return ApiResult<ReceptionAppointment>.Failed(await ReadErrorAsync(response, cancellationToken));
        }

        var appointment = await response.Content.ReadFromJsonAsync<ReceptionAppointment>(cancellationToken);
        return appointment is null
            ? ApiResult<ReceptionAppointment>.Failed("The server returned an empty response.")
            : ApiResult<ReceptionAppointment>.Success(appointment);
    }

    private static async Task<string> ReadErrorAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return "You must sign in before using Reception features.";
        }

        if (response.StatusCode == HttpStatusCode.Forbidden)
        {
            return "Your account does not have Receptionist permission.";
        }

        try
        {
            var json = await response.Content.ReadFromJsonAsync<JsonElement>(cancellationToken);
            if (json.TryGetProperty("detail", out var detail) && !string.IsNullOrWhiteSpace(detail.GetString()))
            {
                return detail.GetString()!;
            }

            if (json.TryGetProperty("title", out var title) && !string.IsNullOrWhiteSpace(title.GetString()))
            {
                return title.GetString()!;
            }
        }
        catch (JsonException)
        {
        }

        return $"Request failed with status {(int)response.StatusCode}.";
    }

}
