using HospitalAppointment.Api.Models.Entities;

namespace HospitalAppointment.Api.Repositories;

public interface IAuthRepository
{
    Task<User?> GetUserByUsernameOrEmailAsync(
        string usernameOrEmail,
        CancellationToken cancellationToken = default);
}