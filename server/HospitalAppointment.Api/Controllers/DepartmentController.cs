
using HospitalAppointment.Api.DTOs.Department;
using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "MedicalServiceManager")]
public class DepartmentController : ControllerBase
{
    private readonly IDepartmentService _departmentService;

    public DepartmentController(IDepartmentService departmentService)
    {
        _departmentService = departmentService;
    }

    // 1. GET: api/Department
    // Xem danh sách, tìm kiếm và lọc Department
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? keyword,
        [FromQuery] bool? isActive,
        CancellationToken cancellationToken)
    {
        var departments = await _departmentService.GetAllAsync(
            keyword,
            isActive,
            cancellationToken);

        return Ok(departments);
    }

    // 2. GET: api/Department/active
    // Lấy danh sách Department đang hoạt động
    [HttpGet("active")]
    [AllowAnonymous]
    public async Task<IActionResult> GetActive(
        CancellationToken cancellationToken)
    {
        var departments = await _departmentService.GetActiveAsync(
            cancellationToken);

        return Ok(departments);
    }

    // 3. GET: api/Department/{id}
    // Xem chi tiết Department
    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(
        int id,
        CancellationToken cancellationToken)
    {
        try
        {
            var department = await _departmentService.GetByIdAsync(
                id,
                cancellationToken);

            return Ok(department);
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

    // 4. POST: api/Department
    // Tạo Department mới
    [HttpPost]
    public async Task<IActionResult> Create(
        [FromBody] CreateDepartmentDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var department = await _departmentService.CreateAsync(
                request,
                cancellationToken);

            return CreatedAtAction(
                nameof(GetById),
                new { id = department.DepartmentId },
                department);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    // 5. PUT: api/Department/{id}
    // Cập nhật Department
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(
        int id,
        [FromBody] UpdateDepartmentDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var department = await _departmentService.UpdateAsync(
                id,
                request,
                cancellationToken);

            return Ok(department);
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

    // 6.  api/Department/{id}/status
    // Kích hoạt / vô hiệu hóa Department
    [HttpPatch("{id:int}/status")]
    public async Task<IActionResult> UpdateStatus(
        int id,
        [FromBody] UpdateDepartmentStatusDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var department = await _departmentService.UpdateStatusAsync(
                id,
                request.IsActive,
                cancellationToken);

            return Ok(department);
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
}
