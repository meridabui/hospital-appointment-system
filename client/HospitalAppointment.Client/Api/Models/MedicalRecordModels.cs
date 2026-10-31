namespace HospitalAppointment.Client.Api.Models;

public class MedicalRecordDetail
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
    public int? MedicalRecordId { get; set; }
    public string? RecordStatus { get; set; }
    public string? Diagnosis { get; set; }
    public string? Result { get; set; }
    public string? TreatmentNotes { get; set; }
    public PrescriptionForm? Prescription { get; set; }
    public List<OrderedServiceForm> OrderedServices { get; set; } = new();
}

public class SaveMedicalRecordForm
{
    public string? Diagnosis { get; set; }
    public string? Result { get; set; }
    public string? TreatmentNotes { get; set; }
    public PrescriptionForm Prescription { get; set; } = new();
    public List<OrderedServiceForm> OrderedServices { get; set; } = new();
}

public class PrescriptionForm
{
    public string? Notes { get; set; }
    public List<PrescriptionItemForm> Items { get; set; } = new();
}

public class PrescriptionItemForm
{
    public int? PrescriptionItemId { get; set; }
    public string MedicationName { get; set; } = string.Empty;
    public string? Dosage { get; set; }
    public string? Route { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public decimal? Quantity { get; set; }
    public string? Instructions { get; set; }
}

public class OrderedServiceForm
{
    public int? OrderedServiceId { get; set; }
    public int MedicalServiceId { get; set; }
    public string? ServiceName { get; set; }
    public string? Notes { get; set; }
    public string ServiceStatus { get; set; } = "Ordered";
}

public class CompletedVisit
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

public class ActiveMedicalService
{
    public int MedicalServiceId { get; set; }
    public string ServiceCode { get; set; } = null!;
    public string ServiceName { get; set; } = null!;
    public string DepartmentName { get; set; } = null!;
}
