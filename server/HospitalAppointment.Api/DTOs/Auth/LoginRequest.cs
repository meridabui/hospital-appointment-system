using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.DTOs.Auth;

public class LoginRequest
{
    [Required(ErrorMessage = "Vui lòng nhập tài khoản.")]
    public string Username { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập mật khẩu.")]
    public string Password { get; set; } = null!;
}