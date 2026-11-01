using System.Net;
using HospitalAppointment.Client.Core;
using HospitalAppointment.Client.Models.Department;

namespace HospitalAppointment.Client.Services;

public class DepartmentApiService
{
    private readonly ApiClient _api;

    public DepartmentApiService(ApiClient api)
    {
        _api = api;
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

        var result = await _api.GetAsync<List<DepartmentDto>>(url);
        if (!result.IsSuccess)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to load departments.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data ?? new List<DepartmentDto>();
    }

    // 2. Get active departments
    public async Task<List<DepartmentDto>> GetActiveAsync()
    {
        var result = await _api.GetAsync<List<DepartmentDto>>("api/Department/active");
        if (!result.IsSuccess)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to load active departments.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data ?? new List<DepartmentDto>();
    }

    // 3. Get by ID
    public async Task<DepartmentDto?> GetByIdAsync(int id)
    {
        var result = await _api.GetAsync<DepartmentDto>($"api/Department/{id}");
        if (!result.IsSuccess)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to load department detail.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data;
    }

    // 4. Create
    public async Task<DepartmentDto> CreateAsync(
        CreateDepartmentDto request)
    {
        var result = await _api.PostAsync<CreateDepartmentDto, DepartmentDto>(
            "api/Department", request);

        if (!result.IsSuccess || result.Data == null)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to create department.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data;
    }

    // 5. Update
    public async Task<DepartmentDto> UpdateAsync(
        int id,
        UpdateDepartmentDto request)
    {
        var result = await _api.PutAsync<UpdateDepartmentDto, DepartmentDto>(
            $"api/Department/{id}", request);

        if (!result.IsSuccess || result.Data == null)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to update department.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data;
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

        var result = await _api.PatchAsync<UpdateDepartmentStatusDto, DepartmentDto>(
            $"api/Department/{id}/status", request);

        if (!result.IsSuccess || result.Data == null)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to update department status.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data;
    }
}
