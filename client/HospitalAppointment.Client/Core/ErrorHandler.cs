using System.Net;
using System.Net.Http.Json;
using System.Text.Json;

namespace HospitalAppointment.Client.Core;

public static class ErrorHandler
{
    public static async Task<ApiException> ToApiExceptionAsync(HttpResponseMessage response)
    {
        return new ApiException(response.StatusCode, await ReadMessageAsync(response));
    }

    private static async Task<string> ReadMessageAsync(HttpResponseMessage response)
    {
        if (response.StatusCode == HttpStatusCode.Unauthorized)
        {
            return "Your session is invalid or has expired. Please log in again.";
        }

        try
        {
            var error = await response.Content.ReadFromJsonAsync<ErrorBody>();
            if (!string.IsNullOrWhiteSpace(error?.Message))
            {
                return error.Message;
            }
        }
        catch (Exception ex) when (ex is JsonException or NotSupportedException)
        {
            // The body is not the API error format, so use the default message below.
        }

        return response.StatusCode == HttpStatusCode.Forbidden
            ? "You do not have permission to do this."
            : "Something went wrong. Please try again.";
    }

    private sealed record ErrorBody(string? Message);
}
