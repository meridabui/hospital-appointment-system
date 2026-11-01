using HospitalAppointment.Api.DTOs.MedicalRecords;

namespace HospitalAppointment.Api.DTOs.PatientPortal;

public class MedicalHistoryItemDto
{
    public int AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = null!;
    public int MedicalRecordId { get; set; }
    public DateOnly Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string DepartmentName { get; set; } = null!;
    public string DoctorName { get; set; } = null!;
    public string? Specialty { get; set; }
    public string? Diagnosis { get; set; }
    public string? Result { get; set; }
    public string? TreatmentNotes { get; set; }
    public DateTime? FinalizedAt { get; set; }
}

// Chi tiết = thông tin tóm tắt + đơn thuốc + dịch vụ được chỉ định
public class MedicalHistoryDetailDto : MedicalHistoryItemDto
{
    public PrescriptionDto? Prescription { get; set; }
    public List<OrderedServiceDto> OrderedServices { get; set; } = new();
}
