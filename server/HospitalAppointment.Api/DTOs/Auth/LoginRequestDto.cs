using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    public string UsernameOrEmail { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}