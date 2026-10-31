
using HospitalAppointment.Api.DTOs.Schedule;
using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "SchedulingManager")]
public class ScheduleController : ControllerBase
{
    private readonly IScheduleService _scheduleService;

    public ScheduleController(IScheduleService scheduleService)
    {
        _scheduleService = scheduleService;
    }

    // 1. GET: api/Schedule
    // Lấy danh sách lịch, hỗ trợ lọc

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int? doctorId,
        [FromQuery] int? departmentId,
        [FromQuery] DateOnly? slotDate,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var schedules = await _scheduleService.GetAllAsync(
            doctorId,
            departmentId,
            slotDate,
            isActive,
            cancellationToken);

        return Ok(schedules);
    }

    // 2. GET: api/Schedule/{id}
    // Chi tiết một Schedule Slot

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var schedule = await _scheduleService.GetByIdAsync(
                id,
                cancellationToken);

            return Ok(schedule);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // 3. GET: api/Schedule/doctors
    // Lấy danh sách Doctor

    [HttpGet("doctors")]
    public async Task<IActionResult> GetDoctors(
        CancellationToken cancellationToken)
    {
        var doctors = await _scheduleService.GetDoctorsAsync(
            cancellationToken);

        return Ok(doctors);
    }

    // 4. GET: api/Schedule/doctors/{doctorId}/departments
    // Lấy Department được phân công cho Doctor

    [HttpGet("doctors/{doctorId:int}/departments")]
    public async Task<IActionResult> GetDepartmentsByDoctor(
        int doctorId,
        CancellationToken cancellationToken)
    {
        try
        {
            var departments =
                await _scheduleService.GetDepartmentsByDoctorAsync(
                    doctorId,
                    cancellationToken);

            return Ok(departments);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
    }

    // 5. POST: api/Schedule
    // Tạo lịch và tự động sinh Schedule Slots

    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateScheduleDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            // Lấy UserId từ JWT
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var createdBy) ||
                createdBy <= 0)
            {
                return Unauthorized(new
                {
                    message = "Invalid user information in token."
                });
            }

            var schedules = await _scheduleService.CreateAsync(
                request,
                createdBy,
                cancellationToken);

            return StatusCode(
                StatusCodes.Status201Created,
                schedules);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // 6. PUT: api/Schedule/{id}
    // Chỉnh sửa Schedule Slot

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateScheduleDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var schedule = await _scheduleService.UpdateAsync(
                id,
                request,
                cancellationToken);

            return Ok(schedule);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // 7. PATCH: api/Schedule/{id}/status
    // Kích hoạt / vô hiệu hóa Schedule Slot

    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateScheduleStatusDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var schedule = await _scheduleService.UpdateStatusAsync(
                id,
                request.IsActive,
                cancellationToken);

            return Ok(schedule);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }
}
