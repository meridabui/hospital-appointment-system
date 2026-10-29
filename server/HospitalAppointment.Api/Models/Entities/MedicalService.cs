namespace HospitalAppointment.Api.Models.Entities;

public class MedicalService
{
    public int MedicalServiceId { get; set; }
    public int DepartmentId { get; set; }
    public string ServiceCode { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string? Description { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }

    public Department Department { get; set; } = null!;
    public ICollection<OrderedService> OrderedServices { get; set; } = new List<OrderedService>();
}
