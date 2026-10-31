using HospitalAppointment.Api.Models.Enums;

namespace HospitalAppointment.Api.Models.Entities;

public class Appointment
{
    public int AppointmentId { get; set; }
    public string AppointmentCode { get; set; } = null!;
    public int PatientId { get; set; }
    public int DoctorId { get; set; }
    public int DepartmentId { get; set; }
    public int ScheduleSlotId { get; set; }
    public AppointmentStatus Status { get; set; } = AppointmentStatus.Pending;
    public string? BookingReason { get; set; }
    public string? RejectionReason { get; set; }
    public string? CancellationReason { get; set; }
    public string? NoShowReason { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? ConfirmedAt { get; set; }
    public int? ConfirmedBy { get; set; }
    public DateTime? CheckedInAt { get; set; }
    public int? CheckedInBy { get; set; }
    public DateTime? ReadyAt { get; set; }
    public DateTime? ConsultationStartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
    public int? CompletedBy { get; set; }
    public DateTime? CancelledAt { get; set; }
    public int? CancelledBy { get; set; }
    public DateTime? NoShowAt { get; set; }
    public int? NoShowBy { get; set; }
    public bool IsActiveBooking { get; set; } = true;
    public byte[] RowVersion { get; set; } = Array.Empty<byte>();

    public Patient Patient { get; set; } = null!;
    public Doctor Doctor { get; set; } = null!;
    public Department Department { get; set; } = null!;
    public DoctorScheduleSlot ScheduleSlot { get; set; } = null!;
    public User? ConfirmedByUser { get; set; }
    public User? CheckedInByUser { get; set; }
    public User? CompletedByUser { get; set; }
    public User? CancelledByUser { get; set; }
    public User? NoShowByUser { get; set; }
    public MedicalRecord? MedicalRecord { get; set; }

    /// <summary>
    /// Thao tác Check-in theo Class Diagram 3.7.1.1 (Confirmed -> CheckedIn)
    /// </summary>
    public void CheckIn(int userId)
    {
        if (Status != AppointmentStatus.Confirmed)
        {
            throw new InvalidOperationException($"Lịch hẹn không thể check-in vì đang ở trạng thái {Status}.");
        }

        Status = AppointmentStatus.CheckedIn;
        CheckedInAt = DateTime.UtcNow;
        CheckedInBy = userId;
    }

    /// <summary>
    /// Kiểm tra điều kiện để đánh dấu No-show theo Class Diagram 3.7.1.1
    /// </summary>
    public bool CanMarkAsNoShow()
    {
        return Status == AppointmentStatus.Confirmed;
    }

    /// <summary>
    /// Thao tác đánh dấu No-show theo Class Diagram 3.7.1.1 (Confirmed -> NoShow)
    /// </summary>
    public void MarkAsNoShow(int userId, string? reason = null)
    {
        if (!CanMarkAsNoShow())
        {
            throw new InvalidOperationException($"Không thể đánh dấu No-show cho lịch hẹn ở trạng thái {Status}.");
        }

        Status = AppointmentStatus.NoShow;
        NoShowAt = DateTime.UtcNow;
        NoShowBy = userId;
        NoShowReason = reason;
        IsActiveBooking = false;
    }

    /// <summary>
    /// Kiểm tra điều kiện để bác sĩ bắt đầu khám (ReadyForConsultation)
    /// </summary>
    public bool CanStartConsultation() => Status == AppointmentStatus.ReadyForConsultation;

    /// <summary>
    /// Bác sĩ bắt đầu khám (ReadyForConsultation -> InConsultation)
    /// </summary>
    public void StartConsultation()
    {
        if (!CanStartConsultation())
        {
            throw new InvalidOperationException($"Không thể bắt đầu khám vì lịch hẹn đang ở trạng thái {Status}.");
        }

        Status = AppointmentStatus.InConsultation;
        ConsultationStartedAt = DateTime.UtcNow;
    }
}