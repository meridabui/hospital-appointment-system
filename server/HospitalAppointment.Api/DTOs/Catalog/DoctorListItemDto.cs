namespace HospitalAppointment.Api.DTOs.Catalog;

public class DoctorListItemDto
{
    public int DoctorId { get; set; }
    public string FullName { get; set; } = null!;
    public string? Specialty { get; set; }
    public int DepartmentId { get; set; }
    public int AvailableSlotCount { get; set; }
    public DateOnly? NextAvailableDate { get; set; }
}