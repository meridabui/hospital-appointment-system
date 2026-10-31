
using HospitalAppointment.Api.DTOs.MedicalService;
using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "MedicalServiceManager")]
public class MedicalServiceController : ControllerBase
{
    private readonly IMedicalServiceService _medicalServiceService;

    public MedicalServiceController(
        IMedicalServiceService medicalServiceService)
    {
        _medicalServiceService = medicalServiceService;
    }

    // 1. GET: api/MedicalService
    // Lấy danh sách, tìm kiếm và lọc dịch vụ
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? keyword,
        [FromQuery] int? departmentId,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var services = await _medicalServiceService.GetAllAsync(
            keyword,
            departmentId,
            isActive,
            cancellationToken);

        return Ok(services);
    }

    // 2. GET: api/MedicalService/active
    // Lấy dịch vụ đang hoạt động
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive(
        [FromQuery] int? departmentId,
        CancellationToken cancellationToken)
    {
        var services = await _medicalServiceService.GetActiveAsync(
            departmentId,
            cancellationToken);

        return Ok(services);
    }

    // 3. GET: api/MedicalService/{id}
    // Xem chi tiết dịch vụ
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var service = await _medicalServiceService.GetByIdAsync(
                id,
                cancellationToken);

            return Ok(service);
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

    // 4. POST: api/MedicalService
    // Tạo dịch vụ mới
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateMedicalServiceDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var service = await _medicalServiceService.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = service.MedicalServiceId },
                service);
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

    // 5. PUT: api/MedicalService/{id}
    // Cập nhật dịch vụ
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateMedicalServiceDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var service = await _medicalServiceService.UpdateAsync(
                id,
                request,
                cancellationToken);

            return Ok(service);
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

    // 6. PATCH: api/MedicalService/{id}/status
    // Kích hoạt / vô hiệu hóa dịch vụ
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateMedicalServiceStatusDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var service = await _medicalServiceService.UpdateStatusAsync(
                id,
                request.IsActive,
                cancellationToken);

            return Ok(service);
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
