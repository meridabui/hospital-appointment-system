using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.DTOs.MedicalRecords;

public class SaveMedicalRecordRequest
{
    public string? Diagnosis { get; set; }
    public string? Result { get; set; }
    public string? TreatmentNotes { get; set; }

    // null = giữ nguyên đơn thuốc; không có thuốc = xóa đơn thuốc
    public PrescriptionRequest? Prescription { get; set; }

    // null = giữ nguyên chỉ định; danh sách = đồng bộ theo danh sách này
    public List<OrderedServiceRequest>? OrderedServices { get; set; }
}

public class PrescriptionRequest
{
    [MaxLength(1000)]
    public string? Notes { get; set; }

    public List<PrescriptionItemRequest> Items { get; set; } = new();
}

public class PrescriptionItemRequest
{
    public int? PrescriptionItemId { get; set; }

    [Required(ErrorMessage = "Tên thuốc là bắt buộc.")]
    [MaxLength(200)]
    public string MedicationName { get; set; } = null!;

    [MaxLength(100)]
    public string? Dosage { get; set; }

    [MaxLength(100)]
    public string? Route { get; set; }

    [MaxLength(200)]
    public string? Frequency { get; set; }

    [MaxLength(100)]
    public string? Duration { get; set; }

    [Range(typeof(decimal), "0.01", "99999999.99", ErrorMessage = "Số lượng phải lớn hơn 0.")]
    public decimal? Quantity { get; set; }

    [MaxLength(500)]
    public string? Instructions { get; set; }
}

public class OrderedServiceRequest
{
    public int? OrderedServiceId { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Dịch vụ không hợp lệ.")]
    public int MedicalServiceId { get; set; }

    [MaxLength(500)]
    public string? Notes { get; set; }
}
