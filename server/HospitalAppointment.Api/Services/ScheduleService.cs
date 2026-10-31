
using HospitalAppointment.Api.DTOs.Schedule;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using HospitalAppointment.Api.Repositories;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Services;

public class ScheduleService : IScheduleService
{
    private readonly IScheduleRepository _scheduleRepository;

    public ScheduleService(IScheduleRepository scheduleRepository)
    {
        _scheduleRepository = scheduleRepository;
    }

    // =========================================
    // 1. GET ALL / FILTER
    // =========================================

    public async Task<List<DoctorScheduleDto>> GetAllAsync(
        int? doctorId = null,
        int? departmentId = null,
        DateOnly? slotDate = null,
        bool? isActive = null,
        CancellationToken cancellationToken = default)
    {
        var slots = await _scheduleRepository.GetAllAsync(
            doctorId,
            departmentId,
            slotDate,
            isActive,
            cancellationToken);

        return slots.Select(MapToDto).ToList();
    }

    // =========================================
    // 2. GET BY ID
    // =========================================

    public async Task<DoctorScheduleDto> GetByIdAsync(
        int scheduleSlotId,
        CancellationToken cancellationToken = default)
    {
        if (scheduleSlotId <= 0)
            throw new ArgumentException("Invalid schedule slot ID.");

        var slot = await _scheduleRepository.GetByIdAsync(
            scheduleSlotId,
            cancellationToken);

        if (slot == null)
            throw new KeyNotFoundException("Schedule slot not found.");

        return MapToDto(slot);
    }

    // =========================================
    // 3. GET DOCTORS
    // =========================================

    public async Task<List<DoctorOptionDto>> GetDoctorsAsync(
        CancellationToken cancellationToken = default)
    {
        var doctors = await _scheduleRepository.GetDoctorsAsync(
            cancellationToken);

        return doctors.Select(d => new DoctorOptionDto
        {
            DoctorId = d.DoctorId,
            DoctorName = d.Staff.User.FullName,
            Specialty = d.Specialty
        }).ToList();
    }

    // =========================================
    // 4. GET DEPARTMENTS BY DOCTOR
    // =========================================

    public async Task<List<DepartmentOptionDto>>
        GetDepartmentsByDoctorAsync(
            int doctorId,
            CancellationToken cancellationToken = default)
    {
        if (doctorId <= 0)
            throw new ArgumentException("Invalid doctor ID.");

        var doctor = await _scheduleRepository.GetDoctorByIdAsync(
            doctorId,
            cancellationToken);

        if (doctor == null)
            throw new KeyNotFoundException("Doctor not found.");

        var departments =
            await _scheduleRepository.GetDepartmentsByDoctorAsync(
                doctorId,
                cancellationToken);

        return departments.Select(d => new DepartmentOptionDto
        {
            DepartmentId = d.DepartmentId,
            DepartmentName = d.DepartmentName
        }).ToList();
    }

    // =========================================
    // 5. CREATE SCHEDULE
    // =========================================

    public async Task<List<DoctorScheduleDto>> CreateAsync(
        CreateScheduleDto request,
        int createdBy,
        CancellationToken cancellationToken = default)
    {
        if (createdBy <= 0)
            throw new ArgumentException("Invalid user ID.");

        if (request.DoctorId <= 0 || request.DepartmentId <= 0)
            throw new ArgumentException("Doctor and Department are required.");

        if (request.SlotDate < DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("Cannot create a schedule in the past.");

        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("End time must be later than start time.");

        if (request.SlotDurationMinutes <= 0)
            throw new ArgumentException("Invalid slot duration.");

        var totalMinutes =
            (request.EndTime.ToTimeSpan() -
             request.StartTime.ToTimeSpan()).TotalMinutes;

        if (totalMinutes % request.SlotDurationMinutes != 0)
        {
            throw new ArgumentException(
                "Working time must be divisible by slot duration.");
        }

        // Kiểm tra Doctor
        var doctor = await _scheduleRepository.GetDoctorByIdAsync(
            request.DoctorId,
            cancellationToken);

        if (doctor == null)
            throw new KeyNotFoundException("Doctor not found.");

        if (doctor.DoctorStatus != DoctorStatus.Active ||
            doctor.Staff.StaffStatus != StaffStatus.Active ||
            !doctor.Staff.User.IsActive)
        {
            throw new InvalidOperationException(
                "Doctor is not currently active.");
        }

        // Kiểm tra Department được phân công
        var isAssigned =
            await _scheduleRepository.IsDoctorAssignedToDepartmentAsync(
                request.DoctorId,
                request.DepartmentId,
                cancellationToken);

        if (!isAssigned)
        {
            throw new InvalidOperationException(
                "Doctor is not assigned to this department.");
        }

        // Sinh danh sách slot trong bộ nhớ
        var slots = new List<DoctorScheduleSlot>();

        var currentTime = request.StartTime;

        while (currentTime < request.EndTime)
        {
            var nextTime = currentTime.AddMinutes(
                request.SlotDurationMinutes);

            if (nextTime > request.EndTime)
                break;

            // Kiểm tra xung đột với lịch đã tồn tại
            var hasOverlap = await _scheduleRepository.HasOverlapAsync(
                request.DoctorId,
                request.SlotDate,
                currentTime,
                nextTime,
                cancellationToken: cancellationToken);

            if (hasOverlap)
            {
                throw new InvalidOperationException(
                    $"Schedule conflict at {currentTime:HH:mm} - {nextTime:HH:mm}.");
            }

            slots.Add(new DoctorScheduleSlot
            {
                DoctorId = request.DoctorId,
                DepartmentId = request.DepartmentId,
                SlotDate = request.SlotDate,
                StartTime = currentTime,
                EndTime = nextTime,
                IsActive = true,
                CreatedBy = createdBy,
                CreatedAt = DateTime.UtcNow
            });

            currentTime = nextTime;
        }

        if (slots.Count == 0)
            throw new InvalidOperationException("No valid schedule slots generated.");

        // Lưu toàn bộ slot
        try
        {
            await _scheduleRepository.AddRangeAsync(
                slots,
                cancellationToken);

            await _scheduleRepository.SaveChangesAsync(
                cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException(
                "Schedule conflicts with existing data. Please refresh and try again.");
        }

        // Trả về danh sách đã tạo
        var result = new List<DoctorScheduleDto>();

        foreach (var slot in slots)
        {
            var savedSlot = await _scheduleRepository.GetByIdAsync(
                slot.ScheduleSlotId,
                cancellationToken);

            if (savedSlot != null)
                result.Add(MapToDto(savedSlot));
        }

        return result;
    }

    // =========================================
    // 6. UPDATE SCHEDULE SLOT
    // =========================================

    public async Task<DoctorScheduleDto> UpdateAsync(
        int scheduleSlotId,
        UpdateScheduleDto request,
        CancellationToken cancellationToken = default)
    {
        if (scheduleSlotId <= 0)
            throw new ArgumentException("Invalid schedule slot ID.");

        if (request.DoctorId <= 0 || request.DepartmentId <= 0)
            throw new ArgumentException("Doctor and Department are required.");

        if (request.EndTime <= request.StartTime)
            throw new ArgumentException("End time must be later than start time.");

        var slot = await _scheduleRepository.GetByIdAsync(
            scheduleSlotId,
            cancellationToken);

        if (slot == null)
            throw new KeyNotFoundException("Schedule slot not found.");

        // Không chỉnh sửa slot đang có Appointment
        if (await _scheduleRepository.HasActiveAppointmentAsync(
            scheduleSlotId,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "Cannot edit a schedule slot that has an active appointment.");
        }

        var doctor = await _scheduleRepository.GetDoctorByIdAsync(
            request.DoctorId,
            cancellationToken);

        if (doctor == null)
            throw new KeyNotFoundException("Doctor not found.");

        if (doctor.DoctorStatus != DoctorStatus.Active ||
            doctor.Staff.StaffStatus != StaffStatus.Active ||
            !doctor.Staff.User.IsActive)
        {
            throw new InvalidOperationException(
                "Doctor is not currently active.");
        }

        if (!await _scheduleRepository.IsDoctorAssignedToDepartmentAsync(
            request.DoctorId,
            request.DepartmentId,
            cancellationToken))
        {
            throw new InvalidOperationException(
                "Doctor is not assigned to this department.");
        }

        if (request.SlotDate < DateOnly.FromDateTime(DateTime.Today))
            throw new ArgumentException("Cannot move a schedule to the past.");

        // Kiểm tra slot khác có giao nhau không
        var hasOverlap = await _scheduleRepository.HasOverlapAsync(
            request.DoctorId,
            request.SlotDate,
            request.StartTime,
            request.EndTime,
            scheduleSlotId,
            cancellationToken);

        if (hasOverlap)
        {
            throw new InvalidOperationException(
                "The updated schedule overlaps with another schedule.");
        }

        slot.DoctorId = request.DoctorId;
        slot.DepartmentId = request.DepartmentId;
        slot.SlotDate = request.SlotDate;
        slot.StartTime = request.StartTime;
        slot.EndTime = request.EndTime;

        try
        {
            await _scheduleRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException(
                "Schedule conflicts with existing data.");
        }

        return MapToDto(slot);
    }

    // =========================================
    // 7. ACTIVATE / DEACTIVATE
    // =========================================

    public async Task<DoctorScheduleDto> UpdateStatusAsync(
        int scheduleSlotId,
        bool isActive,
        CancellationToken cancellationToken = default)
    {
        if (scheduleSlotId <= 0)
            throw new ArgumentException("Invalid schedule slot ID.");

        var slot = await _scheduleRepository.GetByIdAsync(
            scheduleSlotId,
            cancellationToken);

        if (slot == null)
            throw new KeyNotFoundException("Schedule slot not found.");

        if (slot.IsActive == isActive)
            return MapToDto(slot);

        if (!isActive)
        {
            if (await _scheduleRepository.HasActiveAppointmentAsync(
                scheduleSlotId,
                cancellationToken))
            {
                throw new InvalidOperationException(
                    "Cannot deactivate a schedule slot that has an active appointment.");
            }
        }
        else
        {
            if (slot.SlotDate < DateOnly.FromDateTime(DateTime.Today))
                throw new InvalidOperationException(
                    "Cannot activate a schedule slot in the past.");

            var doctor = await _scheduleRepository.GetDoctorByIdAsync(
                slot.DoctorId,
                cancellationToken);

            if (doctor == null ||
                doctor.DoctorStatus != DoctorStatus.Active ||
                doctor.Staff.StaffStatus != StaffStatus.Active ||
                !doctor.Staff.User.IsActive)
            {
                throw new InvalidOperationException(
                    "Cannot activate a schedule for an inactive doctor.");
            }

            if (!await _scheduleRepository.IsDoctorAssignedToDepartmentAsync(
                slot.DoctorId,
                slot.DepartmentId,
                cancellationToken))
            {
                throw new InvalidOperationException(
                    "Doctor is no longer assigned to this department.");
            }

            var hasOverlap = await _scheduleRepository.HasOverlapAsync(
                slot.DoctorId,
                slot.SlotDate,
                slot.StartTime,
                slot.EndTime,
                slot.ScheduleSlotId,
                cancellationToken);

            if (hasOverlap)
            {
                throw new InvalidOperationException(
                    "Cannot activate this slot because it overlaps another schedule.");
            }
        }

        slot.IsActive = isActive;

        try
        {
            await _scheduleRepository.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException)
        {
            throw new InvalidOperationException(
                "Unable to update schedule status due to a data conflict.");
        }

        return MapToDto(slot);
    }

    // =========================================
    // ENTITY -> DTO
    // =========================================

    private static DoctorScheduleDto MapToDto(
        DoctorScheduleSlot slot)
    {
        return new DoctorScheduleDto
        {
            ScheduleSlotId = slot.ScheduleSlotId,
            DoctorId = slot.DoctorId,
            DoctorName = slot.Doctor.Staff.User.FullName,
            Specialty = slot.Doctor.Specialty,
            DepartmentId = slot.DepartmentId,
            DepartmentName = slot.Department.DepartmentName,
            SlotDate = slot.SlotDate,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime,
            IsActive = slot.IsActive,
            CreatedAt = slot.CreatedAt
        };
    }
}
