using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Họ tên là bắt buộc.")]
    [StringLength(150)]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Ngày sinh là bắt buộc.")]
    public DateOnly? DateOfBirth { get; set; }

    [Required(ErrorMessage = "Số điện thoại là bắt buộc.")]
    [RegularExpression(@"^(0|\+84)\d{9}$", ErrorMessage = "Số điện thoại không hợp lệ.")]
    public string Phone { get; set; } = null!;

    [Required(ErrorMessage = "Email là bắt buộc.")]
    [EmailAddress(ErrorMessage = "Email không đúng định dạng.")]
    [StringLength(254)]
    public string Email { get; set; } = null!;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
    [RegularExpression(@"^(?=.*[A-Za-z])(?=.*\d).{8,100}$",
        ErrorMessage = "Mật khẩu tối thiểu 8 ký tự, gồm chữ và số.")]
    public string Password { get; set; } = null!;

    [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu.")]
    [Compare(nameof(Password), ErrorMessage = "Mật khẩu nhập lại không khớp.")]
    public string ConfirmPassword { get; set; } = null!;

    [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Giới tính phải là Male, Female hoặc Other.")]
    public string? Gender { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }
}