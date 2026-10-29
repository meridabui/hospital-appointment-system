namespace HospitalAppointment.Api.Models.Enums;

public enum AppointmentStatus
{
    Pending,
    Confirmed,
    CheckedIn,
    ReadyForConsultation,
    InConsultation,
    Completed,
    Rejected,
    Cancelled,
    NoShow
}
