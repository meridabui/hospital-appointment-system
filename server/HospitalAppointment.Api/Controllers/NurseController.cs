using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Nurse")]
public class NurseController : ControllerBase
{
    private readonly INurseService _nurseService;

    public NurseController(INurseService nurseService)
    {
        _nurseService = nurseService;
    }

    // 1. Danh sách bệnh nhân đã Check-in
    [HttpGet("appointments/checked-in")]
    public async Task<IActionResult> GetCheckedInAppointments(
        CancellationToken cancellationToken)
    {
        var appointments =
            await _nurseService.GetCheckedInAppointmentsAsync(
                cancellationToken);

        return Ok(appointments);
    }

    // 2. Chi tiết Appointment
    [HttpGet("appointments/{appointmentId:int}")]
    public async Task<IActionResult> GetAppointmentDetail(
        int appointmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var appointment =
                await _nurseService.GetAppointmentDetailAsync(
                    appointmentId,
                    cancellationToken);

            return Ok(appointment);
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

    // 3. MarkReady
    [HttpPost("appointments/{appointmentId:int}/mark-ready")]
    public async Task<IActionResult> MarkReady(
        int appointmentId,
        CancellationToken cancellationToken)
    {
        try
        {
            var userIdClaim = User.FindFirstValue(
                ClaimTypes.NameIdentifier);

            if (!int.TryParse(userIdClaim, out var nurseUserId))
            {
                return Unauthorized(new
                {
                    message = "Invalid nurse identity."
                });
            }

            var appointment = await _nurseService.MarkReadyAsync(
                appointmentId,
                nurseUserId,
                cancellationToken);

            return Ok(new
            {
                message = "Patient is ready for consultation.",
                appointmentId = appointment.AppointmentId,
                appointmentCode = appointment.AppointmentCode,
                status = appointment.Status.ToString(),
                readyAt = appointment.ReadyAt,
                readyBy = appointment.ReadyBy
            });
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