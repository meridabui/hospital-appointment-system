using HospitalAppointment.Api.DTOs.Confirmation;

namespace HospitalAppointment.Api.Services;

public enum ConfirmationFailure
{
    None,
    NotFound,
    InvalidState
}

public sealed record ConfirmationOperationResult(
    ConfirmationAppointmentDto? Appointment,
    ConfirmationFailure Failure,
    string? Error)
{
    public bool IsSuccess => Failure == ConfirmationFailure.None;

    public static ConfirmationOperationResult Success(ConfirmationAppointmentDto appointment) =>
        new(appointment, ConfirmationFailure.None, null);

    public static ConfirmationOperationResult Failed(ConfirmationFailure failure, string error) =>
        new(null, failure, error);
}
