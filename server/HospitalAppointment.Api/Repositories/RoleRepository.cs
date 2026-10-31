using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public interface IRoleRepository
{
    Task<Role?> GetByNameAsync(string roleName);
}

public class RoleRepository : IRoleRepository
{
    private readonly HospitalAppointmentDbContext _db;
    public RoleRepository(HospitalAppointmentDbContext db) => _db = db;

    public Task<Role?> GetByNameAsync(string roleName)
        => _db.Roles.FirstOrDefaultAsync(r => r.RoleName == roleName);
}