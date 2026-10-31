using HospitalAppointment.Api.Models.Entities;

namespace HospitalAppointment.Api.Services;

public interface ITokenService
{
    (string Token, DateTime ExpiresAt) CreateToken(User user);
}