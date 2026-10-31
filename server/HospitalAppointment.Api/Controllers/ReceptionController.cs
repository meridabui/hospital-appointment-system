using HospitalAppointment.Api.Models.DTOs.Reception;
using HospitalAppointment.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReceptionController : ControllerBase
{
    private readonly IAppointmentReceptionService _receptionService;
    private readonly ILogger<ReceptionController> _logger;

    public ReceptionController(
        IAppointmentReceptionService receptionService,
        ILogger<ReceptionController> logger)
    {
        _receptionService = receptionService;
        _logger = logger;
    }

    /// <summary>
    /// Tìm kiếm và tra cứu lịch hẹn tiếp nhận tại quầy (theo mã hẹn, tên BN, SĐT hoặc ngày khám)
    /// </summary>
    [HttpGet("appointments")]
    [ProducesResponseType(typeof(List<ReceptionAppointmentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> SearchAppointments([FromQuery] string? keyword, [FromQuery] DateOnly? date)
    {
        var result = await _receptionService.SearchAppointmentsAsync(keyword, date);
        return Ok(result);
    }

    /// <summary>
    /// Tiếp nhận và Check-in cho bệnh nhân khi đến quầy theo Class Diagram 3.7.1.1
    /// </summary>
    [HttpPost("appointments/{id:int}/check-in")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> CheckInAppointment(int id, [FromBody] CheckInRequest? request)
    {
        try
        {
            // Mặc định lấy UserId nếu có truyền, hoặc UserID mặc định của quầy lễ tân (id = 4 theo seed Role Receptionist)
            int userId = request?.UserId ?? 4;

            var success = await _receptionService.CheckInAppointmentAsync(id, userId);
            if (!success)
            {
                return BadRequest(new { message = "Không thể thực hiện Check-in cho lịch hẹn này." });
            }

            return Ok(new { message = "Bệnh nhân đã được Check-in thành công. Đã đưa vào hàng đợi khám." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xử lý check-in lịch hẹn {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Lỗi hệ thống khi xử lý check-in." });
        }
    }

    /// <summary>
    /// Đánh dấu bệnh nhân vắng mặt (No-show) theo Class Diagram 3.7.1.1
    /// </summary>
    [HttpPost("appointments/{id:int}/no-show")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> MarkNoShow(int id, [FromBody] MarkNoShowRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            int userId = request.UserId ?? 4;

            var success = await _receptionService.MarkNoShowAsync(id, userId, request.Reason);
            if (!success)
            {
                return BadRequest(new { message = "Không thể đánh dấu No-show cho lịch hẹn này." });
            }

            return Ok(new { message = "Đã đánh dấu lịch hẹn vắng mặt (No-show) thành công." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Lỗi khi xử lý No-show lịch hẹn {Id}", id);
            return StatusCode(StatusCodes.Status500InternalServerError, new { message = "Lỗi hệ thống khi xử lý No-show." });
        }
    }
}

