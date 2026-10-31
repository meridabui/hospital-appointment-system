using HospitalAppointment.Api.Common.Helpers;
using HospitalAppointment.Api.Common.Security;
using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Data;

public static class DevDataSeeder
{
    public static async Task SeedAsync(HospitalAppointmentDbContext db)
    {
        await SeedStaticDataAsync(db);
        await SeedSlotsAsync(db);
        await SeedReceptionDemoAsync(db);
    }

    private static async Task SeedStaticDataAsync(HospitalAppointmentDbContext db)
    {
        if (await db.Departments.AnyAsync()) return;

        var roles = await db.Roles.ToDictionaryAsync(r => r.RoleName, r => r.RoleId);

        var cardio = NewDepartment("CARD", "Cardiology", "Khám và điều trị tim mạch", true);
        var derma = NewDepartment("DERM", "Dermatology", "Khám da liễu", true);
        var ortho = NewDepartment("ORTH", "Orthopedics", "Khám cơ xương khớp (chưa có bác sĩ - dùng để test)", true);
        var neuro = NewDepartment("NEUR", "Neurology", "Thần kinh (đã vô hiệu hóa - dùng để test)", false);
        db.Departments.AddRange(cardio, derma, ortho, neuro);

        // Receptionist: receptionist / Recep@123
        var recepUser = NewUser("receptionist", "receptionist@hospital.local", "Recep@123",
            "Lê Thị Hoa", roles["Receptionist"], "0900000001", true);
        db.Staff.Add(new Staff
        {
            User = recepUser,
            EmployeeCode = "REC001",
            JobTitle = "Receptionist",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
            StaffStatus = StaffStatus.Active
        });

        // Doctor: mật khẩu chung Doctor@123
        AddDoctor(db, roles["Doctor"], "doctor.a", "Nguyễn Văn A", "DOC001", "Interventional Cardiology", DoctorStatus.Active, cardio, "0900000011");
        AddDoctor(db, roles["Doctor"], "doctor.b", "Trần Thị B", "DOC002", "Echocardiography", DoctorStatus.Active, cardio, "0900000012");
        AddDoctor(db, roles["Doctor"], "doctor.c", "Lê Văn C", "DOC003", "Dermatology", DoctorStatus.Active, derma, "0900000013");
        AddDoctor(db, roles["Doctor"], "doctor.d", "Phạm Văn D", "DOC004", "General Cardiology", DoctorStatus.Inactive, cardio, "0900000014"); // bác sĩ bị vô hiệu hóa
        AddDoctor(db, roles["Doctor"], "doctor.e", "Hoàng Văn E", "DOC005", "Neurology", DoctorStatus.Active, neuro, "0900000015");      // thuộc khoa bị vô hiệu hóa

        await db.SaveChangesAsync();
    }

    private static async Task SeedSlotsAsync(HospitalAppointmentDbContext db)
    {
        var today = DateOnly.FromDateTime(ClockHelper.VietnamNow);
        var until = today.AddDays(14);

        var starts = new List<TimeOnly>();
        for (var t = new TimeOnly(8, 0); t <= new TimeOnly(11, 30); t = t.AddMinutes(30)) starts.Add(t);
        for (var t = new TimeOnly(13, 30); t <= new TimeOnly(16, 0); t = t.AddMinutes(30)) starts.Add(t);

        var doctors = await db.Doctors.Include(d => d.DepartmentAssignments).ToListAsync();

        var existing = await db.DoctorScheduleSlots
            .Where(s => s.SlotDate >= today && s.SlotDate <= until)
            .Select(s => new { s.DoctorId, s.SlotDate, s.StartTime })
            .ToListAsync();
        var keys = existing.Select(x => (x.DoctorId, x.SlotDate, x.StartTime)).ToHashSet();

        foreach (var doctor in doctors)
        {
            var assignment = doctor.DepartmentAssignments.FirstOrDefault();
            if (assignment == null) continue;

            for (var i = 0; i <= 14; i++)
            {
                var date = today.AddDays(i);
                if (date.DayOfWeek == DayOfWeek.Sunday) continue;

                foreach (var start in starts)
                {
                    if (!keys.Add((doctor.DoctorId, date, start))) continue;

                    db.DoctorScheduleSlots.Add(new DoctorScheduleSlot
                    {
                        DoctorId = doctor.DoctorId,
                        DepartmentId = assignment.DepartmentId,
                        SlotDate = date,
                        StartTime = start,
                        EndTime = start.AddMinutes(30),
                        IsActive = true,
                        CreatedAt = DateTime.UtcNow
                    });
                }
            }
        }

        await db.SaveChangesAsync();
    }

    private static async Task SeedReceptionDemoAsync(HospitalAppointmentDbContext db)
    {
        const string checkInCode = "AP-DEMO-CHECKIN";
        const string noShowCode = "AP-DEMO-NOSHOW";

        if (await db.Appointments.AnyAsync(a =>
                a.AppointmentCode == checkInCode || a.AppointmentCode == noShowCode))
        {
            return;
        }

        var receptionist = await db.Users.SingleAsync(u => u.Username == "receptionist");
        var doctor = await db.Doctors
            .Include(d => d.DepartmentAssignments)
            .FirstAsync(d => d.DoctorStatus == DoctorStatus.Active && d.DepartmentAssignments.Any());
        var departmentId = doctor.DepartmentAssignments.First().DepartmentId;
        var today = DateOnly.FromDateTime(ClockHelper.VietnamNow);

        var patient = await db.Patients.SingleOrDefaultAsync(p => p.PatientCode == "PT-DEMO-RECEPTION");
        if (patient is null)
        {
            patient = new Patient
            {
                PatientCode = "PT-DEMO-RECEPTION",
                FullName = "Bệnh nhân Demo Tiếp nhận",
                Phone = "0900999999",
                DateOfBirth = new DateOnly(1990, 1, 15),
                Gender = "Other",
                Address = "Dữ liệu dùng để kiểm thử Development",
                CreatedAt = DateTime.UtcNow
            };
            db.Patients.Add(patient);
        }

        var checkInSlot = new DoctorScheduleSlot
        {
            DoctorId = doctor.DoctorId,
            DepartmentId = departmentId,
            SlotDate = today,
            StartTime = new TimeOnly(17, 0),
            EndTime = new TimeOnly(17, 30),
            IsActive = true,
            CreatedBy = receptionist.UserId,
            CreatedAt = DateTime.UtcNow
        };
        var noShowSlot = new DoctorScheduleSlot
        {
            DoctorId = doctor.DoctorId,
            DepartmentId = departmentId,
            SlotDate = today.AddDays(-1),
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(9, 30),
            IsActive = true,
            CreatedBy = receptionist.UserId,
            CreatedAt = DateTime.UtcNow
        };

        db.Appointments.AddRange(
            new Appointment
            {
                AppointmentCode = checkInCode,
                Patient = patient,
                DoctorId = doctor.DoctorId,
                DepartmentId = departmentId,
                ScheduleSlot = checkInSlot,
                Status = AppointmentStatus.Confirmed,
                BookingReason = "Lịch demo dùng để kiểm thử Check-in trên web",
                ConfirmedAt = DateTime.UtcNow,
                ConfirmedBy = receptionist.UserId,
                CreatedAt = DateTime.UtcNow,
                IsActiveBooking = true
            },
            new Appointment
            {
                AppointmentCode = noShowCode,
                Patient = patient,
                DoctorId = doctor.DoctorId,
                DepartmentId = departmentId,
                ScheduleSlot = noShowSlot,
                Status = AppointmentStatus.Confirmed,
                BookingReason = "Lịch demo dùng để kiểm thử No-show trên web",
                ConfirmedAt = DateTime.UtcNow,
                ConfirmedBy = receptionist.UserId,
                CreatedAt = DateTime.UtcNow,
                IsActiveBooking = true
            });

        await db.SaveChangesAsync();
    }

    private static Department NewDepartment(string code, string name, string description, bool isActive) => new()
    {
        DepartmentCode = code,
        DepartmentName = name,
        Description = description,
        IsActive = isActive,
        CreatedAt = DateTime.UtcNow
    };

    private static User NewUser(string username, string email, string password, string fullName,
        int roleId, string phone, bool isActive) => new()
        {
            RoleId = roleId,
            Username = username,
            Email = email,
            PasswordHash = PasswordHelper.Hash(password),
            Phone = phone,
            FullName = fullName,
            IsActive = isActive,
            CreatedAt = DateTime.UtcNow
        };

    private static void AddDoctor(HospitalAppointmentDbContext db, int doctorRoleId, string username,
        string fullName, string employeeCode, string specialty, DoctorStatus status, Department department, string phone)
    {
        var active = status == DoctorStatus.Active;
        var user = NewUser(username, $"{username}@hospital.local", "Doctor@123", fullName, doctorRoleId, phone, active);
        var staff = new Staff
        {
            User = user,
            EmployeeCode = employeeCode,
            JobTitle = "Doctor",
            HireDate = DateOnly.FromDateTime(DateTime.UtcNow),
            StaffStatus = active ? StaffStatus.Active : StaffStatus.Inactive
        };
        var doctor = new Doctor { Staff = staff, Specialty = specialty, DoctorStatus = status };
        doctor.DepartmentAssignments.Add(new DoctorDepartment
        {
            Department = department,
            AssignedAt = DateTime.UtcNow,
            IsPrimary = true
        });
        db.Doctors.Add(doctor);
    }
}
