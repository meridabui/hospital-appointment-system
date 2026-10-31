namespace HospitalAppointment.Api.DTOs.Reception;

public sealed record ReceptionAppointmentDto(
    int AppointmentId,
    string AppointmentCode,
    string Status,
    int PatientId,
    string PatientCode,
    string PatientName,
    string? PatientPhone,
    DateOnly? PatientDateOfBirth,
    string? PatientGender,
    string DoctorName,
    string? DoctorSpecialty,
    string DepartmentName,
    DateOnly SlotDate,
    TimeOnly StartTime,
    TimeOnly EndTime,
    string? BookingReason,
    DateTime? CheckedInAt,
    DateTime? NoShowAt,
    string? NoShowReason);
