using HospitalAppointment.Api.DTOs.Appointments;

namespace HospitalAppointment.Api.Services;

public interface IBookingService
{
    Task<AppointmentResponse> CreateAsync(int userId, CreateAppointmentRequest request);
}