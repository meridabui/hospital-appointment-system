using HospitalAppointment.Api.Models.Entities;

namespace HospitalAppointment.Api.Services;

public interface INotificationService
{
    Task SendAppointmentConfirmedAsync(Appointment appointment);
    Task SendAppointmentCancelledAsync(Appointment appointment);
    Task SendAppointmentNoShowAsync(Appointment appointment);
    Task SendAppointmentRejectedAsync(Appointment appointment);
    Task SendAppointmentBookedAsync(Appointment appointment);
}