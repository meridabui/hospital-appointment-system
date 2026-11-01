using System.Net;
using HospitalAppointment.Client.Core;
using HospitalAppointment.Client.Models.MedicalService;

namespace HospitalAppointment.Client.Services;

public class MedicalServiceApiService
{
    private readonly ApiClient _api;

    public MedicalServiceApiService(ApiClient api)
    {
        _api = api;
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

        var result = await _api.GetAsync<List<MedicalServiceDto>>(url);
        if (!result.IsSuccess)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to load medical services.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data ?? new List<MedicalServiceDto>();
    }

    // 2. Get active services
    public async Task<List<MedicalServiceDto>> GetActiveAsync(
        int? departmentId = null)
    {
        var url = "api/MedicalService/active";

        if (departmentId.HasValue)
            url += $"?departmentId={departmentId.Value}";

        var result = await _api.GetAsync<List<MedicalServiceDto>>(url);
        if (!result.IsSuccess)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to load active medical services.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data ?? new List<MedicalServiceDto>();
    }

    // 3. Get by ID
    public async Task<MedicalServiceDto?> GetByIdAsync(int id)
    {
        var result = await _api.GetAsync<MedicalServiceDto>($"api/MedicalService/{id}");
        if (!result.IsSuccess)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to load medical service detail.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data;
    }

    // 4. Create
    public async Task<MedicalServiceDto> CreateAsync(
        CreateMedicalServiceDto request)
    {
        var result = await _api.PostAsync<CreateMedicalServiceDto, MedicalServiceDto>(
            "api/MedicalService", request);

        if (!result.IsSuccess || result.Data == null)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to create medical service.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data;
    }

    // 5. Update
    public async Task<MedicalServiceDto> UpdateAsync(
        int id,
        UpdateMedicalServiceDto request)
    {
        var result = await _api.PutAsync<UpdateMedicalServiceDto, MedicalServiceDto>(
            $"api/MedicalService/{id}", request);

        if (!result.IsSuccess || result.Data == null)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to update medical service.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data;
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

        var result = await _api.PatchAsync<UpdateMedicalServiceStatusDto, MedicalServiceDto>(
            $"api/MedicalService/{id}/status", request);

        if (!result.IsSuccess || result.Data == null)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to update medical service status.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data;
    }
}
