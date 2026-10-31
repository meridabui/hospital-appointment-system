
using HospitalAppointment.Client.Models.Department;
using System.Net.Http.Json;

namespace HospitalAppointment.Client.Services;

public class DepartmentApiService
{
    private readonly HttpClient _httpClient;

    public DepartmentApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // 1. Get all / Search / Filter
    public async Task<List<DepartmentDto>> GetAllAsync(
        string? keyword = null,
        bool? isActive = null)
    {
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(keyword))
            query.Add($"keyword={Uri.EscapeDataString(keyword)}");

        if (isActive.HasValue)
            query.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");

        var url = "api/Department";

        if (query.Count > 0)
            url += "?" + string.Join("&", query);

        return await _httpClient.GetFromJsonAsync<List<DepartmentDto>>(url)
            ?? new List<DepartmentDto>();
    }

    // 2. Get active departments
    public async Task<List<DepartmentDto>> GetActiveAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<DepartmentDto>>(
            "api/Department/active") ?? new List<DepartmentDto>();
    }

    // 3. Get by ID
    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<DepartmentDto>(
            $"api/Department/{id}");
    }

    // 4. Create
    public async Task<DepartmentDto> CreateAsync(
        CreateDepartmentDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/Department", request);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<DepartmentDto>())!;
    }

    // 5. Update
    public async Task<DepartmentDto> UpdateAsync(
        int id,
        UpdateDepartmentDto request)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/Department/{id}", request);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<DepartmentDto>())!;
    }

    // 6. Activate / Deactivate
    public async Task<DepartmentDto> UpdateStatusAsync(
        int id,
        bool isActive)
    {
        var request = new UpdateDepartmentStatusDto
        {
            IsActive = isActive
        };

        var response = await _httpClient.PatchAsJsonAsync(
            $"api/Department/{id}/status", request);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<DepartmentDto>())!;
    }
}
