using HospitalAppointment.Api.Common.Constants;
using HospitalAppointment.Api.DTOs.Catalog;
using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/departments")]
[Authorize(Roles = RoleNames.PatientOrReceptionist)]
public class DepartmentsController : ControllerBase
{
    private readonly ICatalogService _catalog;

    public DepartmentsController(ICatalogService catalog) => _catalog = catalog;

    [HttpGet]
    public async Task<ActionResult<List<DepartmentDto>>> GetDepartments()
        => Ok(await _catalog.GetDepartmentsAsync());

    [HttpGet("{departmentId:int}/doctors")]
    public async Task<ActionResult<List<DoctorListItemDto>>> GetDoctors(int departmentId)
        => Ok(await _catalog.GetDoctorsByDepartmentAsync(departmentId));
}