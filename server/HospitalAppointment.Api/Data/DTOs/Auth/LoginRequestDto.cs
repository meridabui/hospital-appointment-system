using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.Data.DTOs.Auth;

public class LoginRequestDto
{
    [Required]
    public string UsernameOrEmail { get; set; } = null!;

    [Required]
    public string Password { get; set; } = null!;
}