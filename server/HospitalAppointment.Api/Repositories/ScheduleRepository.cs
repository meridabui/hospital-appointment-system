
using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Repositories;

public class ScheduleRepository : IScheduleRepository
{
    private readonly HospitalAppointmentDbContext _context;

    public ScheduleRepository(HospitalAppointmentDbContext context)
    {
        _context = context;
    }

    // 1. GET ALL / FILTER SCHEDULE SLOTS

    public async Task<List<DoctorScheduleSlot>> GetAllAsync(
        int? doctorId = null,
        int? departmentId = null,
        DateOnly? slotDate = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DoctorScheduleSlots
            .AsNoTracking()
            .Include(s => s.Doctor)
                .ThenInclude(d => d.Staff)
                    .ThenInclude(st => st.User)
            .Include(s => s.Department)
            .AsQueryable();

        if (doctorId.HasValue)
        {
            query = query.Where(
                s => s.DoctorId == doctorId.Value);
        }

        if (departmentId.HasValue)
        {
            query = query.Where(
                s => s.DepartmentId == departmentId.Value);
        }

        if (slotDate.HasValue)
        {
            query = query.Where(
                s => s.SlotDate == slotDate.Value);
        }

        if (isActive.HasValue)
        {
            query = query.Where(
                s => s.IsActive == isActive.Value);
        }

        return await query
            .OrderBy(s => s.SlotDate)
            .ThenBy(s => s.StartTime)
            .ThenBy(s => s.DoctorId)
            .ToListAsync(cancellationToken);
    }

    // 2. GET SCHEDULE SLOT BY ID

    public async Task<DoctorScheduleSlot?> GetByIdAsync(
        int scheduleSlotId,
        CancellationToken cancellationToken = default)
    {
        return await _context.DoctorScheduleSlots
            .Include(s => s.Doctor)
                .ThenInclude(d => d.Staff)
                    .ThenInclude(st => st.User)
            .Include(s => s.Department)
            .FirstOrDefaultAsync(
                s => s.ScheduleSlotId == scheduleSlotId,
                cancellationToken);
    }

    // 3. GET ACTIVE DOCTORS

    public async Task<List<Doctor>> GetDoctorsAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .AsNoTracking()
            .Include(d => d.Staff)
                .ThenInclude(s => s.User)
            .Where(d =>
                d.DoctorStatus == DoctorStatus.Active &&
                d.Staff.StaffStatus == StaffStatus.Active &&
                d.Staff.User.IsActive)
            .OrderBy(d => d.Staff.User.FullName)
            .ToListAsync(cancellationToken);
    }

    // 4. GET DOCTOR BY ID

    public async Task<Doctor?> GetDoctorByIdAsync(
        int doctorId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Doctors
            .Include(d => d.Staff)
                .ThenInclude(s => s.User)
            .FirstOrDefaultAsync(
                d => d.DoctorId == doctorId,
                cancellationToken);
    }

    // 5. GET DEPARTMENTS ASSIGNED TO DOCTOR

    public async Task<List<Department>> GetDepartmentsByDoctorAsync(
        int doctorId,
        CancellationToken cancellationToken = default)
    {
        return await _context.DoctorDepartments
            .AsNoTracking()
            .Where(dd =>
                dd.DoctorId == doctorId &&
                dd.Department.IsActive)
            .Select(dd => dd.Department)
            .OrderBy(d => d.DepartmentName)
            .ToListAsync(cancellationToken);
    }

    // 6. CHECK DOCTOR-DEPARTMENT ASSIGNMENT

    public async Task<bool> IsDoctorAssignedToDepartmentAsync(
        int doctorId,
        int departmentId,
        CancellationToken cancellationToken = default)
    {
        return await _context.DoctorDepartments
            .AnyAsync(
                dd =>
                    dd.DoctorId == doctorId &&
                    dd.DepartmentId == departmentId &&
                    dd.Department.IsActive,
                cancellationToken);
    }

    // 7. CHECK TIME OVERLAP

    public async Task<bool> HasOverlapAsync(
        int doctorId,
        DateOnly slotDate,
        TimeOnly startTime,
        TimeOnly endTime,
        int? excludeScheduleSlotId = null,
        CancellationToken cancellationToken = default)
    {
        var query = _context.DoctorScheduleSlots
            .AsNoTracking()
            .Where(s =>
                s.DoctorId == doctorId &&
                s.SlotDate == slotDate &&

                // Hai khoảng thời gian giao nhau
                s.StartTime < endTime &&
                s.EndTime > startTime);

        if (excludeScheduleSlotId.HasValue)
        {
            query = query.Where(
                s => s.ScheduleSlotId != excludeScheduleSlotId.Value);
        }

        // Kiểm tra cả slot inactive vì database đang có
        // unique index không lọc theo IsActive.
        return await query.AnyAsync(cancellationToken);
    }

    // 8. CHECK ACTIVE APPOINTMENT

    public async Task<bool> HasActiveAppointmentAsync(
        int scheduleSlotId,
        CancellationToken cancellationToken = default)
    {
        return await _context.Appointments
            .AnyAsync(
                a =>
                    a.ScheduleSlotId == scheduleSlotId &&
                    a.IsActiveBooking,
                cancellationToken);
    }

    // 9. ADD MULTIPLE SCHEDULE SLOTS

    public async Task AddRangeAsync(
        IEnumerable<DoctorScheduleSlot> scheduleSlots,
        CancellationToken cancellationToken = default)
    {
        await _context.DoctorScheduleSlots.AddRangeAsync(
            scheduleSlots,
            cancellationToken);
    }

    // 10. SAVE CHANGES
    public async Task<int> SaveChangesAsync(
        CancellationToken cancellationToken = default)
    {
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
