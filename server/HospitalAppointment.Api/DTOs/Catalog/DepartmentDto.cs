namespace HospitalAppointment.Api.DTOs.Catalog;

public class DepartmentDto
{
    public int DepartmentId { get; set; }
    public string DepartmentCode { get; set; } = null!;
    public string DepartmentName { get; set; } = null!;
    public string? Description { get; set; }
}