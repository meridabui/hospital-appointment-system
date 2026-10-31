using HospitalAppointment.Api.Common.Exceptions;
using HospitalAppointment.Api.Common.Security;
using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.DTOs.Auth;
using HospitalAppointment.Api.Models.Entities;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Services;

public class AuthService : IAuthService
{
    private readonly HospitalAppointmentDbContext _db;
    private readonly ITokenService _tokenService;

    public AuthService(HospitalAppointmentDbContext db, ITokenService tokenService)
    {
        _db = db;
        _tokenService = tokenService;
    }

    public async Task<RegisterResponse> RegisterPatientAsync(RegisterRequest request, int? performedByUserId = null)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var phone = request.Phone.Trim();
        var fullName = request.FullName.Trim();
        var dob = request.DateOfBirth!.Value;

        if (dob > DateOnly.FromDateTime(DateTime.UtcNow) || dob.Year < 1900)
            throw new AppException("Ngày sinh không hợp lệ.");

        // Kiểm tra trùng lặp
        if (await _db.Users.AnyAsync(u => u.Email == email || u.Username == email))
            throw new AppException("Email này đã được đăng ký.", StatusCodes.Status409Conflict);

        if (await _db.Users.AnyAsync(u => u.Phone == phone))
            throw new AppException("Số điện thoại này đã được đăng ký.", StatusCodes.Status409Conflict);

        var role = await _db.Roles.FirstAsync(r => r.RoleName == "Patient");

        var user = new User
        {
            RoleId = role.RoleId,
            Username = email,
            Email = email,
            PasswordHash = PasswordHelper.Hash(request.Password),
            Phone = phone,
            FullName = fullName,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        var patient = new Patient
        {
            User = user,
            PatientCode = GeneratePatientCode(),
            FullName = fullName,
            Phone = phone,
            DateOfBirth = dob,
            Gender = request.Gender,
            Address = request.Address?.Trim(),
            CreatedAt = DateTime.UtcNow
        };

        _db.Patients.Add(patient); // kéo theo cả User

        // Receptionist tạo hộ -> ghi nhận người thực hiện
        if (performedByUserId.HasValue)
        {
            _db.AuditLogs.Add(new AuditLog
            {
                UserId = performedByUserId,
                Action = "CreatePatientAccount",
                EntityName = "Patient",
                EntityId = patient.PatientCode,
                Details = $"Receptionist created patient account for {email}",
                CreatedAt = DateTime.UtcNow
            });
        }

        try
        {
            await _db.SaveChangesAsync(); // 1 lần = 1 transaction
        }
        catch (DbUpdateException ex) when (ex.InnerException is SqlException { Number: 2601 or 2627 })
        {
            // Hai người đăng ký cùng lúc, unique index chặn lại
            throw new AppException("Email hoặc số điện thoại đã được đăng ký.", StatusCodes.Status409Conflict);
        }

        return new RegisterResponse
        {
            UserId = user.UserId,
            PatientId = patient.PatientId,
            PatientCode = patient.PatientCode,
            Email = user.Email,
            Message = "Đăng ký tài khoản thành công. Bạn có thể đăng nhập."
        };
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request)
    {
        var identifier = request.Username.Trim();

        var user = await _db.Users.Include(u => u.Role)
            .FirstOrDefaultAsync(u => u.Username == identifier || u.Email == identifier);

        if (user == null || !PasswordHelper.Verify(request.Password, user.PasswordHash))
            throw new AppException("Tài khoản hoặc mật khẩu không đúng.", StatusCodes.Status401Unauthorized);

        if (!user.IsActive)
            throw new AppException("Tài khoản đã bị khóa.", StatusCodes.Status403Forbidden);

        var (token, expiresAt) = _tokenService.CreateToken(user);

        return new AuthResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt,
            UserId = user.UserId,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.RoleName
        };
    }

    private static string GeneratePatientCode()
        => $"PT{DateTime.UtcNow:yyMMdd}{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
}