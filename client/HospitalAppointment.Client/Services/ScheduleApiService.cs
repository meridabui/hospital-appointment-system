
using HospitalAppointment.Client.Models.Schedule;
using System.Net.Http.Json;

namespace HospitalAppointment.Client.Services;

public class ScheduleApiService
{
    private readonly HttpClient _httpClient;

    public ScheduleApiService(HttpClient httpClient)
    {
        _httpClient = httpClient;
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

        return await _httpClient.GetFromJsonAsync<List<DoctorScheduleDto>>(url)
            ?? new List<DoctorScheduleDto>();
    }

    // 2. GET BY ID
    public async Task<DoctorScheduleDto?> GetByIdAsync(int id)
    {
        return await _httpClient.GetFromJsonAsync<DoctorScheduleDto>(
            $"api/Schedule/{id}");
    }

    // 3. GET DOCTORS
    public async Task<List<DoctorOptionDto>> GetDoctorsAsync()
    {
        return await _httpClient.GetFromJsonAsync<List<DoctorOptionDto>>(
            "api/Schedule/doctors")
            ?? new List<DoctorOptionDto>();
    }

    // 4. GET DEPARTMENTS BY DOCTOR

    public async Task<List<DepartmentOptionDto>>
        GetDepartmentsByDoctorAsync(int doctorId)
    {
        return await _httpClient.GetFromJsonAsync<List<DepartmentOptionDto>>(
            $"api/Schedule/doctors/{doctorId}/departments")
            ?? new List<DepartmentOptionDto>();
    }

    // 5. CREATE SCHEDULE

    public async Task<List<DoctorScheduleDto>> CreateAsync(
        CreateScheduleDto request)
    {
        var response = await _httpClient.PostAsJsonAsync(
            "api/Schedule", request);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<List<DoctorScheduleDto>>()
            ?? new List<DoctorScheduleDto>();
    }

    // 6. UPDATE SCHEDULE

    public async Task<DoctorScheduleDto> UpdateAsync(
        int id,
        UpdateScheduleDto request)
    {
        var response = await _httpClient.PutAsJsonAsync(
            $"api/Schedule/{id}", request);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<DoctorScheduleDto>())!;
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

        var response = await _httpClient.PatchAsJsonAsync(
            $"api/Schedule/{id}/status", request);

        response.EnsureSuccessStatusCode();

        return (await response.Content.ReadFromJsonAsync<DoctorScheduleDto>())!;
    }
}
