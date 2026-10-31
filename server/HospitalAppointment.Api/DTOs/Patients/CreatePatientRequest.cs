using System.ComponentModel.DataAnnotations;

namespace HospitalAppointment.Api.DTOs.Patients;

public class CreatePatientRequest
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

    [RegularExpression("^(Male|Female|Other)$", ErrorMessage = "Giới tính phải là Male, Female hoặc Other.")]
    public string? Gender { get; set; }

    [StringLength(300)]
    public string? Address { get; set; }
}