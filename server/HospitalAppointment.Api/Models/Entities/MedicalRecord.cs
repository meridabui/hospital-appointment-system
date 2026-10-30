using HospitalAppointment.Api.Models.Enums;

namespace HospitalAppointment.Api.Models.Entities;

public class MedicalRecord
{
    public int MedicalRecordId { get; set; }
    public int AppointmentId { get; set; }
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public string? Diagnosis { get; set; }
    public string? Result { get; set; }
    public string? TreatmentNotes { get; set; }
    public RecordStatus RecordStatus { get; set; } = RecordStatus.Draft;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? FinalizedAt { get; set; }
    public int? FinalizedBy { get; set; }

    public Appointment Appointment { get; set; } = null!;
    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public User? FinalizedByUser { get; set; }
    public ICollection<OrderedService> OrderedServices { get; set; } = new List<OrderedService>();
    public Prescription? Prescription { get; set; }
}
