
namespace HospitalAppointment.Client.Models.Department;

public class UpdateDepartmentDto
{
    public string DepartmentCode { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public string? Description { get; set; }
}
