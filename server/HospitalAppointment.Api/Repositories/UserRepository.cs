using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public interface IUserRepository
{
    Task<bool> EmailExistsAsync(string email);
    Task<bool> PhoneExistsAsync(string phone);
    Task<User?> GetByUsernameOrEmailAsync(string identifier);
    void Add(User user);
}

public class UserRepository : IUserRepository
{
    private readonly HospitalAppointmentDbContext _db;
    public UserRepository(HospitalAppointmentDbContext db) => _db = db;

    public Task<bool> EmailExistsAsync(string email)
        => _db.Users.AnyAsync(u => u.Email == email || u.Username == email);

    public Task<bool> PhoneExistsAsync(string phone)
        => _db.Users.AnyAsync(u => u.Phone == phone);

    public Task<User?> GetByUsernameOrEmailAsync(string identifier)
        => _db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == identifier || u.Email == identifier);

    public void Add(User user) => _db.Users.Add(user);
}