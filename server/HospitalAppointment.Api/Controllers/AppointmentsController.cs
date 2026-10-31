using HospitalAppointment.Api.Common.Constants;
using HospitalAppointment.Api.Common.Helpers;
using HospitalAppointment.Api.DTOs.Appointments;
using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HospitalAppointment.Api.Controllers;

[ApiController]
[Route("api/appointments")]
[Authorize(Roles = RoleNames.Patient)]
public class AppointmentsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public AppointmentsController(IBookingService bookingService) => _bookingService = bookingService;

    [HttpPost]
    public async Task<ActionResult<AppointmentResponse>> Create(CreateAppointmentRequest request)
    {
        var result = await _bookingService.CreateAsync(User.GetUserId(), request);
        return StatusCode(StatusCodes.Status201Created, result);
    }
}