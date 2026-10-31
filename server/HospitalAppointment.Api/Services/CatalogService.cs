using HospitalAppointment.Api.Common.Constants;
using HospitalAppointment.Api.Common.Exceptions;
using HospitalAppointment.Api.Common.Helpers;
using HospitalAppointment.Api.DTOs.Catalog;
using HospitalAppointment.Api.Repositories;

namespace HospitalAppointment.Api.Services;

public class CatalogService : ICatalogService
{
    private readonly IUnitOfWork _uow;

    public CatalogService(IUnitOfWork uow) => _uow = uow;

    public async Task<List<DepartmentDto>> GetDepartmentsAsync()
    {
        var departments = await _uow.Departments.GetActiveAsync();
        return departments.Select(d => new DepartmentDto
        {
            DepartmentId = d.DepartmentId,
            DepartmentCode = d.DepartmentCode,
            DepartmentName = d.DepartmentName,
            Description = d.Description
        }).ToList();
    }

    public async Task<List<DoctorListItemDto>> GetDoctorsByDepartmentAsync(int departmentId)
    {
        await EnsureDepartmentActiveAsync(departmentId);
        var (today, time, toDate) = GetWindow();

        var doctors = await _uow.Doctors.GetBookableByDepartmentAsync(departmentId, today, time, toDate);
        var slotDates = await _uow.ScheduleSlots.GetAvailableSlotDatesAsync(
            doctors.Select(d => d.DoctorId).ToList(), departmentId, today, time, toDate);

        var byDoctor = slotDates.GroupBy(x => x.DoctorId).ToDictionary(g => g.Key, g => g.ToList());

        return doctors.Select(d =>
        {
            byDoctor.TryGetValue(d.DoctorId, out var list);
            return new DoctorListItemDto
            {
                DoctorId = d.DoctorId,
                FullName = d.Staff.User.FullName,
                Specialty = d.Specialty,
                DepartmentId = departmentId,
                AvailableSlotCount = list?.Count ?? 0,
                NextAvailableDate = list?.Min(x => x.SlotDate)
            };
        }).ToList();
    }

    public async Task<List<AvailableDateDto>> GetAvailableDatesAsync(int doctorId, int departmentId)
    {
        await EnsureDepartmentActiveAsync(departmentId);
        await EnsureDoctorBookableAsync(doctorId, departmentId);
        var (today, time, toDate) = GetWindow();

        var slots = await _uow.ScheduleSlots.GetAvailableAsync(doctorId, departmentId, today, toDate, today, time);

        return slots.GroupBy(s => s.SlotDate)
            .Select(g => new AvailableDateDto { Date = g.Key, AvailableSlotCount = g.Count() })
            .OrderBy(x => x.Date)
            .ToList();
    }

    public async Task<List<SlotDto>> GetAvailableSlotsAsync(int doctorId, int departmentId, DateOnly date)
    {
        await EnsureDepartmentActiveAsync(departmentId);
        await EnsureDoctorBookableAsync(doctorId, departmentId);
        var (today, time, toDate) = GetWindow();

        if (date < today || date > toDate)
            throw new AppException($"Ngày khám phải nằm trong {BookingRules.BookingWindowDays} ngày tới.");

        var slots = await _uow.ScheduleSlots.GetAvailableAsync(doctorId, departmentId, date, date, today, time);

        return slots.Select(s => new SlotDto
        {
            ScheduleSlotId = s.ScheduleSlotId,
            Date = s.SlotDate,
            StartTime = s.StartTime,
            EndTime = s.EndTime
        }).ToList();
    }

    private static (DateOnly Today, TimeOnly Time, DateOnly ToDate) GetWindow()
    {
        var now = ClockHelper.VietnamNow;
        var today = DateOnly.FromDateTime(now);
        return (today, TimeOnly.FromDateTime(now), today.AddDays(BookingRules.BookingWindowDays));
    }

    private async Task EnsureDepartmentActiveAsync(int departmentId)
    {
        var department = await _uow.Departments.GetByIdAsync(departmentId)
                         ?? throw new AppException("Không tìm thấy khoa.", StatusCodes.Status404NotFound);

        if (!department.IsActive)
            throw new AppException("Khoa này đã ngừng hoạt động, không thể đặt lịch mới.");
    }

    private async Task EnsureDoctorBookableAsync(int doctorId, int departmentId)
    {
        var doctor = await _uow.Doctors.GetActiveInDepartmentAsync(doctorId, departmentId);
        if (doctor == null)
            throw new AppException("Bác sĩ không còn hoạt động hoặc không thuộc khoa đã chọn.");
    }
}