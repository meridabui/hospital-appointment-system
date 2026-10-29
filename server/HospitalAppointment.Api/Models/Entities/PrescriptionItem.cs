namespace HospitalAppointment.Api.Models.Entities;

public class PrescriptionItem
{
    public int PrescriptionItemId { get; set; }
    public int PrescriptionId { get; set; }
    public string MedicationName { get; set; } = null!;
    public string? Dosage { get; set; }
    public string? Route { get; set; }
    public string? Frequency { get; set; }
    public string? Duration { get; set; }
    public decimal? Quantity { get; set; }
    public string? Instructions { get; set; }

    public Prescription Prescription { get; set; } = null!;
}
