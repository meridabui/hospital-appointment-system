
using HospitalAppointment.Client.Models.Auth;
using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace HospitalAppointment.Client.Services;

public class AuthApiService
{
    private readonly HttpClient _httpClient;

    public LoginResponseDto? CurrentUser { get; private set; }

    public bool IsLoggedIn => CurrentUser != null;

    public AuthApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // LOGIN
    public async Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/Auth/login",
            request);

        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException(
                "Invalid username or password.");
        }

        var result = await response.Content
            .ReadFromJsonAsync<LoginResponseDto>();

        if (result == null ||
            string.IsNullOrWhiteSpace(result.Token))
        {
            throw new InvalidOperationException(
                "JWT Token was not received from server.");
        }

        // Save account information in memory
        CurrentUser = result;

        // Attach JWT to subsequent API requests
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", result.Token);

        return result;
    }

    // LOGOUT
    public void Logout()
    {
        CurrentUser = null;

        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}
