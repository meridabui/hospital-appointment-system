using HospitalAppointment.Api.DTOs.Auth;

namespace HospitalAppointment.Api.Services;

public interface IAuthService
{
    Task<RegisterResponse> RegisterPatientAsync(RegisterRequest request, int? performedByUserId = null);
    Task<AuthResponse> LoginAsync(LoginRequest request);
}