using HospitalAppointment.Api.Common.Constants;
using HospitalAppointment.Api.Common.Helpers;
using HospitalAppointment.Api.DTOs.MedicalRecords;
using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/medical-records")]
[Authorize(Roles = RoleNames.Doctor)]
public class MedicalRecordController : ControllerBase
{
    private readonly IMedicalRecordService _medicalRecordService;

    public MedicalRecordController(IMedicalRecordService medicalRecordService)
    {
        _medicalRecordService = medicalRecordService;
    }

    [HttpGet("services")]
    public async Task<ActionResult<List<ActiveMedicalServiceDto>>> GetActiveServices()
    {
        return Ok(await _medicalRecordService.GetActiveServicesAsync());
    }

    [HttpGet("completed")]
    public async Task<ActionResult<List<CompletedVisitDto>>> GetCompleted([FromQuery] string? keyword, [FromQuery] DateOnly? date)
    {
        return Ok(await _medicalRecordService.GetCompletedVisitsAsync(keyword, date, User.GetUserId()));
    }

    [HttpPost("{appointmentId:int}/complete")]
    public async Task<ActionResult<MedicalRecordDto>> Complete(int appointmentId)
    {
        return Ok(await _medicalRecordService.CompleteConsultationAsync(appointmentId, User.GetUserId()));
    }

    [HttpGet("{appointmentId:int}")]
    public async Task<ActionResult<MedicalRecordDto>> Get(int appointmentId)
    {
        return Ok(await _medicalRecordService.GetAsync(appointmentId, User.GetUserId()));
    }

    [HttpPut("{appointmentId:int}")]
    public async Task<ActionResult<MedicalRecordDto>> Save(int appointmentId, SaveMedicalRecordRequest request)
    {
        return Ok(await _medicalRecordService.SaveAsync(appointmentId, request, User.GetUserId()));
    }
}
