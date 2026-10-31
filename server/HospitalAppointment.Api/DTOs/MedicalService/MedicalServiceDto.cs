namespace HospitalAppointment.Api.DTOs.MedicalService;

public class MedicalServiceDto
{
    public int MedicalServiceId { get; set; }

    public int DepartmentId { get; set; }

    public string DepartmentName { get; set; } = null!;

    public string ServiceCode { get; set; } = null!;

    public string ServiceName { get; set; } = null!;

    public string? Description { get; set; }

    public bool IsActive { get; set; }

    public DateTime CreatedAt { get; set; }
}