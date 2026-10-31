
using HospitalAppointment.Client.Models.MedicalService;
using System.Net.Http.Json;

namespace HospitalAppointment.Client.Services;

public class MedicalServiceApiService
{
    private readonly HttpClient _httpClient;

    public MedicalServiceApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    // 1. Get all / Search / Filter
    public async Task<List<MedicalServiceDto>> GetAllAsync(
        string? keyword = null,
        int? departmentId = null,
        bool? isActive = null)
    {
        var query = new List<string>();

        if (!string.IsNullOrWhiteSpace(keyword))
            query.Add($"keyword={Uri.EscapeDataString(keyword)}");

        if (departmentId.HasValue)
            query.Add($"departmentId={departmentId.Value}");

        if (isActive.HasValue)
            query.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");

        var url = "api/MedicalService";

        if (query.Count > 0)
            url += "?" + string.Join("&", query);

        return await _httpClient.GetFromJsonAsync<List<MedicalServiceDto>>(url)
            ?? new List<MedicalServiceDto>();
    }

    // 2. Get active services
    public async Task<List<MedicalServiceDto>> GetActiveAsync(
        int? departmentId = null)
    {
        var url = "api/MedicalService/active";

        if (departmentId.HasValue)
            url += $"?departmentId={departmentId.Value}";

        return await _httpClient.GetFromJsonAsync<List<MedicalServiceDto>>(url)
            ?? new List<MedicalServiceDto>();
    }

    // 3. Get by ID
    public async Task<MedicalServiceDto?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<MedicalServiceDto>(
            $"api/MedicalService/{id}");
    }

    // 4. Create
    public async Task<MedicalServiceDto> CreateAsync(
        CreateMedicalServiceDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/MedicalService", request);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<MedicalServiceDto>())!;
    }

    // 5. Update
    public async Task<MedicalServiceDto> UpdateAsync(
        int id,
        UpdateMedicalServiceDto request)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/MedicalService/{id}", request);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<MedicalServiceDto>())!;
    }

    // 6. Activate / Deactivate
    public async Task<MedicalServiceDto> UpdateStatusAsync(
        int id,
        bool isActive)
    {
        var request = new UpdateMedicalServiceStatusDto
        {
            IsActive = isActive
        };

        var response = await _httpClient.PatchAsJsonAsync(
            $"api/MedicalService/{id}/status", request);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<MedicalServiceDto>())!;
    }
}
