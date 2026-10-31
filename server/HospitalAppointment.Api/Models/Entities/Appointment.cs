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
    public int? ReadyBy { get; set; }

    public User? ReadyByUser { get; set; }
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

    public void CheckIn(int receptionistUserId, DateTime checkedInAtUtc)
    {
        if (Status != AppointmentStatus.Confirmed)
        {
            throw new InvalidOperationException("Only a confirmed appointment can be checked in.");
        }

        Status = AppointmentStatus.CheckedIn;
        CheckedInBy = receptionistUserId;
        CheckedInAt = checkedInAtUtc;
    }

    public void MarkNoShow(int receptionistUserId, DateTime markedAtUtc, string reason)
    {
        if (Status != AppointmentStatus.Confirmed)
        {
            throw new InvalidOperationException("Only a confirmed appointment can be marked as no-show.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new ArgumentException("A no-show reason is required.", nameof(reason));
        }

        Status = AppointmentStatus.NoShow;
        NoShowBy = receptionistUserId;
        NoShowAt = markedAtUtc;
        NoShowReason = reason.Trim();
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

    /// <summary>
    /// Kiểm tra điều kiện để bác sĩ hoàn tất lượt khám (InConsultation)
    /// </summary>
    public bool CanComplete() => Status == AppointmentStatus.InConsultation;

    /// <summary>
    /// Bác sĩ hoàn tất lượt khám (InConsultation -> Completed).
    /// Nhận UserId vì CompletedBy tham chiếu Users.UserId.
    /// </summary>
    public void Complete(int userId)
    {
        if (!CanComplete())
        {
            throw new InvalidOperationException($"Không thể hoàn tất vì lịch hẹn đang ở trạng thái {Status}.");
        }

        Status = AppointmentStatus.Completed;
        CompletedAt = DateTime.UtcNow;
        CompletedBy = userId;
    }
}
