using System.Security.Cryptography;
using HospitalAppointment.Api.DTOs.Auth;
using HospitalAppointment.Api.DTOs.Patients;

namespace HospitalAppointment.Api.Services;

public class PatientService : IPatientService
{
    private readonly IAuthService _authService;

    public PatientService(IAuthService authService) => _authService = authService;

    public async Task<CreatePatientResponse> CreateByReceptionistAsync(CreatePatientRequest request, int receptionistUserId)
    {
        var tempPassword = GenerateTemporaryPassword();

        var result = await _authService.RegisterPatientAsync(new RegisterRequest
        {
            FullName = request.FullName,
            DateOfBirth = request.DateOfBirth,
            Phone = request.Phone,
            Email = request.Email,
            Gender = request.Gender,
            Address = request.Address,
            Password = tempPassword,
            ConfirmPassword = tempPassword
        }, receptionistUserId);

        return new CreatePatientResponse
        {
            UserId = result.UserId,
            PatientId = result.PatientId,
            PatientCode = result.PatientCode,
            Email = result.Email,
            TemporaryPassword = tempPassword,
            Message = "Đã tạo hồ sơ bệnh nhân. Hãy đưa mật khẩu tạm cho bệnh nhân."
        };
    }

    // 6 chữ + 4 số: luôn thỏa quy tắc mật khẩu (>= 8 ký tự, có chữ và số)
    private static string GenerateTemporaryPassword()
    {
        const string letters = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz";
        const string digits = "23456789";
        var chars = new List<char>();
        for (var i = 0; i < 6; i++) chars.Add(letters[RandomNumberGenerator.GetInt32(letters.Length)]);
        for (var i = 0; i < 4; i++) chars.Add(digits[RandomNumberGenerator.GetInt32(digits.Length)]);
        return new string(chars.ToArray());
    }
}