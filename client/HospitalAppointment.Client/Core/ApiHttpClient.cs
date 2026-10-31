using System.Net;
using System.Net.Http.Json;

namespace HospitalAppointment.Client.Core;

public class ApiHttpClient
{
    private readonly HttpClient _httpClient;

    public ApiHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    public async Task<T> GetAsync<T>(string url)
    {
        using var response = await SendAsync(() => _httpClient.GetAsync(url));
        return (await response.Content.ReadFromJsonAsync<T>())!;
    }

    public async Task PostAsync(string url)
    {
        using var response = await SendAsync(() => _httpClient.PostAsync(url, null));
    }

    private static async Task<HttpResponseMessage> SendAsync(Func<Task<HttpResponseMessage>> send)
    {
        HttpResponseMessage response;
        try
        {
            response = await send();
        }
        catch (HttpRequestException)
        {
            throw new ApiException(HttpStatusCode.ServiceUnavailable, "Cannot connect to the server. Please try again.");
        }

        if (!response.IsSuccessStatusCode)
        {
            using (response)
            {
                throw await ErrorHandler.ToApiExceptionAsync(response);
            }
        }

        return response;
    }
}
