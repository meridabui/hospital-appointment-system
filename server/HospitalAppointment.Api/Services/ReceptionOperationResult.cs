using HospitalAppointment.Api.DTOs.Reception;

namespace HospitalAppointment.Api.Services;

public enum ReceptionFailure
{
    None,
    NotFound,
    InvalidState,
    TooEarlyForNoShow,
    ConcurrencyConflict
}

public sealed record ReceptionOperationResult(
    ReceptionAppointmentDto? Appointment,
    ReceptionFailure Failure,
    string? Error)
{
    public bool IsSuccess => Failure == ReceptionFailure.None;

    public static ReceptionOperationResult Success(ReceptionAppointmentDto appointment) =>
        new(appointment, ReceptionFailure.None, null);

    public static ReceptionOperationResult Failed(ReceptionFailure failure, string error) =>
        new(null, failure, error);
}
