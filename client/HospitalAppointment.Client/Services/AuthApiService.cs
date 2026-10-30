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

    public async Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/Auth/login",
            request);

        if (!response.IsSuccessStatusCode)
        {
            throw new UnauthorizedAccessException(
                "Tên đăng nhập hoặc mật khẩu không chính xác.");
        }

        var result = await response.Content
            .ReadFromJsonAsync<LoginResponseDto>();

        if (result == null || string.IsNullOrWhiteSpace(result.Token))
        {
            throw new InvalidOperationException(
                "Không nhận được JWT Token từ Server.");
        }

        // Lưu thông tin tài khoản trong bộ nhớ
        CurrentUser = result;

        // Tự động gửi JWT trong các request tiếp theo
        _httpClient.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", result.Token);

        return result;
    }

    public void Logout()
    {
        CurrentUser = null;

        _httpClient.DefaultRequestHeaders.Authorization = null;
    }
}