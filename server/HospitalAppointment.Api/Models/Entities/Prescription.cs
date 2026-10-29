namespace HospitalAppointment.Api.Models.Entities;

public class Prescription
{
    public int PrescriptionId { get; set; }
    public int MedicalRecordId { get; set; }
    public DateTime PrescribedAt { get; set; }
    public string? Notes { get; set; }

    public MedicalRecord MedicalRecord { get; set; } = null!;
    public ICollection<PrescriptionItem> Items { get; set; } = new List<PrescriptionItem>();
}
