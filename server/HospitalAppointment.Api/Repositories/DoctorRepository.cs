using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public class DoctorRepository : IDoctorRepository
{
    private readonly HospitalAppointmentDbContext _context;

    public DoctorRepository(HospitalAppointmentDbContext context)
    {
        _context = context;
    }

    public Task<Doctor?> GetActiveByUserIdAsync(int userId)
    {
        return _context.Doctors
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Staff.UserId == userId
                && x.Staff.User.IsActive
                && x.Staff.StaffStatus == StaffStatus.Active
                && x.DoctorStatus == DoctorStatus.Active);
    }
}
