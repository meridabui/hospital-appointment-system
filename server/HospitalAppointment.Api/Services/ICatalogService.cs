using HospitalAppointment.Api.DTOs.Catalog;

namespace HospitalAppointment.Api.Services;

public interface ICatalogService
{
    Task<List<DepartmentDto>> GetDepartmentsAsync();
    Task<List<DoctorListItemDto>> GetDoctorsByDepartmentAsync(int departmentId);
    Task<List<AvailableDateDto>> GetAvailableDatesAsync(int doctorId, int departmentId);
    Task<List<SlotDto>> GetAvailableSlotsAsync(int doctorId, int departmentId, DateOnly date);
}