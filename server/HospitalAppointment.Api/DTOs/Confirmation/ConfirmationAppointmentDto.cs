namespace HospitalAppointment.Api.DTOs.Confirmation;

public sealed record ConfirmationAppointmentDto(
    int AppointmentId,
    string AppointmentCode,
    string Status,
    string PatientName,
    string? PatientPhone,
    string DoctorName,
    string DepartmentName,
    DateOnly SlotDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? BookingReason,
    DateTime? ConfirmedAt,
    string? RejectionReason);
