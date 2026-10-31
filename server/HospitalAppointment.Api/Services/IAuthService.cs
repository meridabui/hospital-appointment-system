using HospitalAppointment.Api.DTOs.Auth;

namespace HospitalAppointment.Api.Services;

public interface IAuthService
{
    Task<LoginResponseDto> LoginAsync(
        LoginRequestDto request,
        CancellationToken cancellationToken = default);
}