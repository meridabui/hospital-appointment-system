using HospitalAppointment.Api.Common.Constants;
using HospitalAppointment.Api.DTOs.Catalog;
using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/doctors")]
[Authorize(Roles = RoleNames.PatientOrReceptionist)]
public class DoctorsController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public DoctorsController(ICatalogService catalog) => _catalog = catalog;

    [HttpGet("{doctorId:int}/available-dates")]
    public async Task<ActionResult<List<AvailableDateDto>>> GetAvailableDates(int doctorId, [FromQuery] int departmentId)
        => Ok(await _catalog.GetAvailableDatesAsync(doctorId, departmentId));

    [HttpGet("{doctorId:int}/slots")]
    public async Task<ActionResult<List<SlotDto>>> GetSlots(
        int doctorId, [FromQuery] int departmentId, [FromQuery] DateOnly date)
        => Ok(await _catalog.GetAvailableSlotsAsync(doctorId, departmentId, date));
}