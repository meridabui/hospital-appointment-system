using System.Net;
using HospitalAppointment.Client.Core;
using HospitalAppointment.Client.Models.Schedule;

namespace HospitalAppointment.Client.Services;

public class ScheduleApiService
{
    private readonly ApiClient _api;

    public ScheduleApiService(ApiClient api)
    {
        _api = api;
    }

    // 1. GET ALL / FILTER SCHEDULES
    public async Task<List<DoctorScheduleDto>> GetAllAsync(
        int? doctorId = null,
        int? departmentId = null,
        DateOnly? slotDate = null,
        bool? isActive = null)
    {
        var query = new List<string>();

        if (doctorId.HasValue)
            query.Add($"doctorId={doctorId.Value}");

        if (departmentId.HasValue)
            query.Add($"departmentId={departmentId.Value}");

        if (slotDate.HasValue)
            query.Add($"slotDate={slotDate.Value:yyyy-MM-dd}");

        if (isActive.HasValue)
            query.Add($"isActive={isActive.Value.ToString().ToLowerInvariant()}");

        var url = "api/Schedule";

        if (query.Count > 0)
            url += "?" + string.Join("&", query);

        var result = await _api.GetAsync<List<DoctorScheduleDto>>(url);
        if (!result.IsSuccess)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to load schedules.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data ?? new List<DoctorScheduleDto>();
    }

    // 2. GET BY ID
    public async Task<DoctorScheduleDto?> GetByIdAsync(int id)
    {
        var result = await _api.GetAsync<DoctorScheduleDto>($"api/Schedule/{id}");
        if (!result.IsSuccess)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to load schedule detail.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data;
    }

    // 3. GET DOCTORS
    public async Task<List<DoctorOptionDto>> GetDoctorsAsync()
    {
        var result = await _api.GetAsync<List<DoctorOptionDto>>("api/Schedule/doctors");
        if (!result.IsSuccess)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to load doctors.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data ?? new List<DoctorOptionDto>();
    }

    // 4. GET DEPARTMENTS BY DOCTOR
    public async Task<List<DepartmentOptionDto>> GetDepartmentsByDoctorAsync(int doctorId)
    {
        var result = await _api.GetAsync<List<DepartmentOptionDto>>(
            $"api/Schedule/doctors/{doctorId}/departments");

        if (!result.IsSuccess)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to load doctor departments.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data ?? new List<DepartmentOptionDto>();
    }

    // 5. CREATE SCHEDULE (Generates slots)
    public async Task<List<DoctorScheduleDto>> CreateAsync(
        CreateScheduleDto request)
    {
        var result = await _api.PostAsync<CreateScheduleDto, List<DoctorScheduleDto>>(
            "api/Schedule", request);

        if (!result.IsSuccess || result.Data == null)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to create schedule.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data;
    }

    // 6. UPDATE SCHEDULE
    public async Task<DoctorScheduleDto> UpdateAsync(
        int id,
        UpdateScheduleDto request)
    {
        var result = await _api.PutAsync<UpdateScheduleDto, DoctorScheduleDto>(
            $"api/Schedule/{id}", request);

        if (!result.IsSuccess || result.Data == null)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to update schedule.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data;
    }

    // 7. ACTIVATE / DEACTIVATE
    public async Task<DoctorScheduleDto> UpdateStatusAsync(
        int id,
        bool isActive)
    {
        var request = new UpdateScheduleStatusDto
        {
            IsActive = isActive
        };

        var result = await _api.PatchAsync<UpdateScheduleStatusDto, DoctorScheduleDto>(
            $"api/Schedule/{id}/status", request);

        if (!result.IsSuccess || result.Data == null)
        {
            throw new HttpRequestException(
                result.ErrorMessage ?? "Unable to update schedule status.",
                null,
                (HttpStatusCode)result.StatusCode);
        }

        return result.Data;
    }
}
