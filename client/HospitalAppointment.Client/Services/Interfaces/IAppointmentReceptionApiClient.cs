using HospitalAppointment.Client.Models.Reception;

namespace HospitalAppointment.Client.Services.Interfaces;

public interface IAppointmentReceptionApiClient
{
    Task<List<ReceptionAppointmentModel>> SearchAppointmentsAsync(string? keyword = null, DateOnly? date = null);
    Task<(bool Success, string Message)> CheckInAppointmentAsync(int appointmentId, int? userId = null);
    Task<(bool Success, string Message)> MarkNoShowAsync(int appointmentId, string reason, int? userId = null);
}

