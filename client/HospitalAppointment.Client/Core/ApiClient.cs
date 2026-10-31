using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.AspNetCore.Components;

namespace HospitalAppointment.Client.Core;

public class ApiResult<T>
{
    public bool IsSuccess { get; init; }
    public T? Data { get; init; }
    public int StatusCode { get; init; }
    public string? ErrorMessage { get; init; }
    public List<string> Errors { get; init; } = new();
}

public class ApiClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _http;
    private readonly AuthSession _session;
    private readonly NavigationManager _nav;

    public ApiClient(HttpClient http, AuthSession session, NavigationManager nav)
    {
        _http = http;
        _session = session;
        _nav = nav;
    }

    public Task<ApiResult<T>> GetAsync<T>(string url)
        => SendAsync<T>(HttpMethod.Get, url, null);

    public Task<ApiResult<TResponse>> PostAsync<TRequest, TResponse>(string url, TRequest body)
        => SendAsync<TResponse>(HttpMethod.Post, url, JsonContent.Create(body, options: JsonOptions));

    private async Task<ApiResult<T>> SendAsync<T>(HttpMethod method, string url, HttpContent? content)
    {
        var token = _session.Token;

        using var request = new HttpRequestMessage(method, url) { Content = content };
        if (token != null)
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        HttpResponseMessage response;
        try
        {
            response = await _http.SendAsync(request);
        }
        catch (HttpRequestException)
        {
            return Fail<T>(0, "Không thể kết nối tới máy chủ. Vui lòng kiểm tra API đã chạy chưa.");
        }

        using (response)
        {
            if (response.IsSuccessStatusCode)
            {
                var data = await response.Content.ReadFromJsonAsync<T>(JsonOptions);
                return new ApiResult<T> { IsSuccess = true, Data = data, StatusCode = (int)response.StatusCode };
            }

            // Đã gửi token mà vẫn 401 -> phiên hết hạn hoặc token không hợp lệ
            if (response.StatusCode == HttpStatusCode.Unauthorized && token != null)
            {
                await _session.SignOutAsync();
                _nav.NavigateToLogin();
                return Fail<T>(401, "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.");
            }

            return await ParseErrorAsync<T>(response);
        }
    }

    private static async Task<ApiResult<T>> ParseErrorAsync<T>(HttpResponseMessage response)
    {
        var status = (int)response.StatusCode;
        var errors = new List<string>();
        string? detail = null;

        try
        {
            var text = await response.Content.ReadAsStringAsync();
            using var doc = JsonDocument.Parse(text);
            var root = doc.RootElement;

            if (root.ValueKind == JsonValueKind.Object)
            {
                // Lỗi validation của [ApiController]: { "errors": { "Field": ["msg"] } }
                if (root.TryGetProperty("errors", out var errs) && errs.ValueKind == JsonValueKind.Object)
                {
                    foreach (var prop in errs.EnumerateObject())
                        foreach (var msg in prop.Value.EnumerateArray())
                            if (msg.GetString() is { Length: > 0 } s) errors.Add(s);
                }

                // Lỗi nghiệp vụ từ middleware: { "detail": "..." }
                if (root.TryGetProperty("detail", out var d) && d.ValueKind == JsonValueKind.String)
                    detail = d.GetString();
            }
        }
        catch (JsonException)
        {
            // Body rỗng hoặc không phải JSON (ví dụ 403 không có nội dung)
        }

        var message = detail
                      ?? (errors.Count > 0 ? errors[0] : DefaultMessage(status));

        if (errors.Count == 0) errors.Add(message);

        return new ApiResult<T> { IsSuccess = false, StatusCode = status, ErrorMessage = message, Errors = errors };
    }

    private static string DefaultMessage(int status) => status switch
    {
        401 => "Bạn chưa đăng nhập.",
        403 => "Bạn không có quyền thực hiện thao tác này.",
        404 => "Không tìm thấy dữ liệu yêu cầu.",
        >= 500 => "Máy chủ gặp lỗi. Vui lòng thử lại sau.",
        _ => "Yêu cầu không hợp lệ."
    };

    private static ApiResult<T> Fail<T>(int status, string message)
        => new() { IsSuccess = false, StatusCode = status, ErrorMessage = message, Errors = { message } };
}