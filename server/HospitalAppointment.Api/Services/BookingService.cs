using HospitalAppointment.Api.Common.Constants;
using HospitalAppointment.Api.Common.Exceptions;
using HospitalAppointment.Api.Common.Helpers;
using HospitalAppointment.Api.DTOs.Appointments;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using HospitalAppointment.Api.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Services;

public class BookingService : IBookingService
{
    private const string SlotTakenMessage =
        "Khung giờ này vừa có người khác đặt. Vui lòng chọn khung giờ khác.";

    private readonly IUnitOfWork _uow;

    public BookingService(IUnitOfWork uow) => _uow = uow;

    public async Task<AppointmentResponse> CreateAsync(int userId, CreateAppointmentRequest request)
    {
        // 1. Patient đã đăng nhập và có quyền đặt lịch (tài khoản đang hoạt động, có hồ sơ Patient)
        var patient = await _uow.Patients.GetActiveByUserIdAsync(userId)
                      ?? throw new AppException("Tài khoản không có quyền đặt lịch.", StatusCodes.Status403Forbidden);

        // Department và Doctor phải đang hoạt động
        var department = await _uow.Departments.GetByIdAsync(request.DepartmentId)
                         ?? throw new AppException("Không tìm thấy khoa.", StatusCodes.Status404NotFound);
        if (!department.IsActive)
            throw new AppException("Khoa này đã ngừng hoạt động, không thể đặt lịch mới.");

        // 2. Doctor đang hoạt động và thuộc khoa đã chọn
        var doctor = await _uow.Doctors.GetActiveInDepartmentAsync(request.DoctorId, request.DepartmentId)
                     ?? throw new AppException("Bác sĩ không còn hoạt động hoặc không thuộc khoa đã chọn.");

        // 3. Slot thuộc lịch làm việc hợp lệ của Doctor
        var slot = await _uow.ScheduleSlots.GetByIdAsync(request.ScheduleSlotId);
        if (slot == null || !slot.IsActive
            || slot.DoctorId != doctor.DoctorId || slot.DepartmentId != department.DepartmentId)
            throw new AppException("Khung giờ không thuộc lịch làm việc hợp lệ của bác sĩ.");

        var now = ClockHelper.VietnamNow;
        var today = DateOnly.FromDateTime(now);
        if (slot.SlotDate.ToDateTime(slot.StartTime) <= now
            || slot.SlotDate > today.AddDays(BookingRules.BookingWindowDays))
            throw new AppException("Khung giờ đã qua hoặc nằm ngoài thời gian cho phép đặt lịch.");

        // 5. Patient không có appointment trùng cùng Doctor và thời gian
        if (await _uow.Appointments.PatientHasActiveAtAsync(
                patient.PatientId, doctor.DoctorId, slot.SlotDate, slot.StartTime))
            throw new AppException("Bạn đã có lịch hẹn với bác sĩ này vào thời gian này.",
                StatusCodes.Status409Conflict);

        // 4. Slot chưa bị appointment khác chiếm giữ (kiểm tra sớm để báo lỗi thân thiện)
        if (await _uow.Appointments.IsSlotTakenAsync(slot.ScheduleSlotId))
            throw new AppException(SlotTakenMessage, StatusCodes.Status409Conflict);

        // 6-7. Tạo Appointment, trạng thái Pending
        var appointment = new Appointment
        {
            AppointmentCode = GenerateAppointmentCode(now),
            PatientId = patient.PatientId,
            DoctorId = doctor.DoctorId,
            DepartmentId = department.DepartmentId,
            ScheduleSlotId = slot.ScheduleSlotId,
            Status = AppointmentStatus.Pending,
            BookingReason = string.IsNullOrWhiteSpace(request.BookingReason) ? null : request.BookingReason.Trim(),
            IsActiveBooking = true,
            CreatedAt = DateTime.UtcNow
        };

        _uow.Appointments.Add(appointment);

        try
        {
            await _uow.SaveChangesAsync();
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 } sql)
        {
            // Hai người gửi cùng lúc: unique index (ScheduleSlotId, IsActiveBooking = 1) chặn người đến sau
            if (sql.Message.Contains("ScheduleSlotId"))
                throw new AppException(SlotTakenMessage, StatusCodes.Status409Conflict);

            throw new AppException("Không thể tạo mã lịch hẹn. Vui lòng thử lại.", StatusCodes.Status409Conflict);
        }

        // 8. Mã Appointment và thông báo
        return new AppointmentResponse
        {
            AppointmentId = appointment.AppointmentId,
            AppointmentCode = appointment.AppointmentCode,
            Status = appointment.Status.ToString(),
            DepartmentName = department.DepartmentName,
            DoctorName = doctor.Staff.User.FullName,
            Date = slot.SlotDate,
            StartTime = slot.StartTime,
            EndTime = slot.EndTime,
            BookingReason = appointment.BookingReason,
            CreatedAt = appointment.CreatedAt,
            Message = "Yêu cầu đặt lịch đã được gửi. Lịch hẹn đang chờ bệnh viện xác nhận."
        };
    }

    private static string GenerateAppointmentCode(DateTime vietnamNow)
        => $"AP{vietnamNow:yyMMdd}{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
}