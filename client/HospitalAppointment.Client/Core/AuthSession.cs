using System.Text.Json;
using HospitalAppointment.Client.Models;
using Microsoft.JSInterop;

namespace HospitalAppointment.Client.Core;

public class AuthSession
{
    private const string StorageKey = "hospital.auth";
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly IJSRuntime _js;
    private Task? _initTask;

    public AuthSession(IJSRuntime js) => _js = js;

    public AuthResponse? Current { get; private set; }

    public bool IsAuthenticated => Current != null && Current.ExpiresAt > DateTime.UtcNow;

    public string? Token => IsAuthenticated ? Current!.AccessToken : null;

    public bool IsInRole(string role)
        => IsAuthenticated && string.Equals(Current!.Role, role, StringComparison.OrdinalIgnoreCase);

    public event Action? Changed;

    // Nhiều component có thể gọi cùng lúc, chỉ nạp từ localStorage đúng 1 lần
    public Task InitializeAsync() => _initTask ??= LoadAsync();

    private async Task LoadAsync()
    {
        try
        {
            var json = await _js.InvokeAsync<string?>("localStorage.getItem", StorageKey);
            if (!string.IsNullOrWhiteSpace(json))
                Current = JsonSerializer.Deserialize<AuthResponse>(json, JsonOptions);
        }
        catch
        {
            Current = null;
        }

        if (Current != null && !IsAuthenticated)
            await SignOutAsync(); // token đã hết hạn
        else
            Changed?.Invoke();
    }

    public async Task SignInAsync(AuthResponse response)
    {
        _initTask ??= Task.CompletedTask;
        Current = response;
        await _js.InvokeVoidAsync("localStorage.setItem", StorageKey, JsonSerializer.Serialize(response, JsonOptions));
        Changed?.Invoke();
    }

    public async Task SignOutAsync()
    {
        Current = null;
        try { await _js.InvokeVoidAsync("localStorage.removeItem", StorageKey); }
        catch { /* bỏ qua */ }
        Changed?.Invoke();
    }
}