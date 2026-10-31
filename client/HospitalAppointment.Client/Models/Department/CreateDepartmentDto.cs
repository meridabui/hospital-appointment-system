
namespace HospitalAppointment.Client.Models.Department;

public class CreateDepartmentDto
{
    public string DepartmentCode { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public string? Description { get; set; }
}
