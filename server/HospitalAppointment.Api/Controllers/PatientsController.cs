using HospitalAppointment.Api.Common.Constants;
using HospitalAppointment.Api.Common.Helpers;
using HospitalAppointment.Api.DTOs.Patients;
using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/patients")]
[Authorize(Roles = RoleNames.Receptionist)]
public class PatientsController : ControllerBase
{
    private readonly IPatientService _patientService;

    public PatientsController(IPatientService patientService) => _patientService = patientService;

    [HttpPost]
    public async Task<ActionResult<CreatePatientResponse>> Create(CreatePatientRequest request)
    {
        var result = await _patientService.CreateByReceptionistAsync(request, User.GetUserId());
        return StatusCode(StatusCodes.Status201Created, result);
    }
}