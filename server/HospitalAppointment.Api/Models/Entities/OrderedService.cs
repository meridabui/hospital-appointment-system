using HospitalAppointment.Api.Models.Enums;

namespace HospitalAppointment.Api.Models.Entities;

public class OrderedService
{
    public int OrderedServiceId { get; set; }
    public int MedicalRecordId { get; set; }
    public int MedicalServiceId { get; set; }
    public int OrderedByDoctorId { get; set; }
    public DateTime OrderedAt { get; set; }
    public string? Notes { get; set; }
    public ServiceStatus ServiceStatus { get; set; } = ServiceStatus.Ordered;

    public MedicalRecord MedicalRecord { get; set; } = null!;
    public MedicalService MedicalService { get; set; } = null!;
    public Doctor OrderedByDoctor { get; set; } = null!;
}
