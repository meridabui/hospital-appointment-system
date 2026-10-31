namespace HospitalAppointment.Api.DTOs.MedicalRecords;

public class MedicalRecordDto
{
    public int AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = null!;
    public string AppointmentStatus { get; set; } = null!;
    public string? BookingReason { get; set; }
    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string DepartmentName { get; set; } = null!;
    public string PatientCode { get; set; } = null!;
    public string PatientName { get; set; } = null!;
    public DateOnly? DateOfBirth { get; set; }
    public string? Gender { get; set; }

    // Các trường dưới đây trống khi chưa có hồ sơ
    public int? MedicalRecordId { get; set; }
    public string? RecordStatus { get; set; }
    public string? Diagnosis { get; set; }
    public string? Result { get; set; }
    public string? TreatmentNotes { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public PrescriptionDto? Prescription { get; set; }
    public List<OrderedServiceDto> OrderedServices { get; set; } = new();
}

public class PrescriptionDto
{
    public string? Notes { get; set; }
    public List<PrescriptionItemDto> Items { get; set; } = new();
}

public class PrescriptionItemDto
{
    public int PrescriptionItemId { get; set; }
    public string MedicationName { get; set; } = null!;
    public string? Dosage { get; set; }
    public string? Route { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public decimal? Quantity { get; set; }
    public string? Instructions { get; set; }
}

public class OrderedServiceDto
{
    public int OrderedServiceId { get; set; }
    public int MedicalServiceId { get; set; }
    public string ServiceName { get; set; } = null!;
    public string? Notes { get; set; }
    public string ServiceStatus { get; set; } = null!;
}

public class CompletedVisitDto
{
    public int AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = null!;
    public DateOnly SlotDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string DepartmentName { get; set; } = null!;
    public string PatientCode { get; set; } = null!;
    public string PatientName { get; set; } = null!;
}

public class ActiveMedicalServiceDto
{
    public int MedicalServiceId { get; set; }
    public string ServiceCode { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string DepartmentName { get; set; } = null!;
}
