using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public class AuthRepository : IAuthRepository
{
    private readonly HospitalAppointmentDbContext _context;

    public AuthRepository(HospitalAppointmentDbContext context)
    {
        _context = context;
    }

    public async Task<User?> GetUserByUsernameOrEmailAsync(
        string usernameOrEmail,
        CancellationToken cancellationToken = default)
    {
        return await _context.Users
            .AsNoTracking()
            .Include(u => u.Role)
            .FirstOrDefaultAsync(
                u => u.Username == usernameOrEmail ||
                     u.Email == usernameOrEmail,
                cancellationToken);
    }
}