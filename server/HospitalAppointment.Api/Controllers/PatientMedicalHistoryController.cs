using HospitalAppointment.Api.Common.Constants;
using HospitalAppointment.Api.Common.Helpers;
using HospitalAppointment.Api.DTOs.Patients;
using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/medical-history")]
[Authorize(Roles = RoleNames.Patient)]
public class PatientMedicalHistoryController : ControllerBase
{
    private readonly IPatientMedicalHistoryService _service;

    public PatientMedicalHistoryController(IPatientMedicalHistoryService service) => _service = service;

    [HttpGet("my")]
    public async Task<ActionResult<List<MedicalHistoryItemDto>>> GetMy()
        => Ok(await _service.GetMyHistoryAsync(User.GetUserId()));

    [HttpGet("my/{appointmentId:int}")]
    public async Task<ActionResult<MedicalHistoryDetailDto>> GetMyDetail(int appointmentId)
        => Ok(await _service.GetMyDetailAsync(User.GetUserId(), appointmentId));
}
