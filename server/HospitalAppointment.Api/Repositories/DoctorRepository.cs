using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public interface IDoctorRepository
{
    // Doctor đang hoạt động, thuộc khoa và còn ít nhất 1 slot khả dụng
    Task<List<Doctor>> GetBookableByDepartmentAsync(int departmentId, DateOnly today, TimeOnly now, DateOnly toDate);

    // Doctor đang hoạt động và thuộc khoa; null nếu không thỏa
    Task<Doctor?> GetActiveInDepartmentAsync(int doctorId, int departmentId);
}

public class DoctorRepository : IDoctorRepository
{
    private readonly HospitalAppointmentDbContext _db;
    public DoctorRepository(HospitalAppointmentDbContext db) => _db = db;

    public Task<List<Doctor>> GetBookableByDepartmentAsync(int departmentId, DateOnly today, TimeOnly now, DateOnly toDate)
    {
        var doctorIdsWithSlots = _db.DoctorScheduleSlots
            .Where(s => s.DepartmentId == departmentId)
            .AvailableForBooking(today, now, toDate)
            .Select(s => s.DoctorId);

        return _db.Doctors.AsNoTracking()
            .Include(d => d.Staff).ThenInclude(s => s.User)
            .Where(d => d.DoctorStatus == DoctorStatus.Active
                     && d.Staff.StaffStatus == StaffStatus.Active
                     && d.Staff.User.IsActive
                     && d.DepartmentAssignments.Any(x => x.DepartmentId == departmentId)
                     && doctorIdsWithSlots.Contains(d.DoctorId))
            .OrderBy(d => d.Staff.User.FullName)
            .ToListAsync();
    }

    public Task<Doctor?> GetActiveInDepartmentAsync(int doctorId, int departmentId)
        => _db.Doctors.AsNoTracking()
            .Include(d => d.Staff).ThenInclude(s => s.User)
            .FirstOrDefaultAsync(d => d.DoctorId == doctorId
                                   && d.DoctorStatus == DoctorStatus.Active
                                   && d.Staff.StaffStatus == StaffStatus.Active
                                   && d.Staff.User.IsActive
                                   && d.DepartmentAssignments.Any(x => x.DepartmentId == departmentId));
}
