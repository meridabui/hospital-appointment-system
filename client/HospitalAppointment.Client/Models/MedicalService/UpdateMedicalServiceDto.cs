
namespace HospitalAppointment.Client.Models.MedicalService;

public class UpdateMedicalServiceDto
{
    public int DepartmentId { get; set; }

    public string ServiceCode { get; set; } = string.Empty;

    public string ServiceName { get; set; } = string.Empty;

    public string? Description { get; set; }
}
