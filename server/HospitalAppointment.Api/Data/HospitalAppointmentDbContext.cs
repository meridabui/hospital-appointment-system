using HospitalAppointment.Api.Models.Entities;
using HospitalAppointment.Api.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace HospitalAppointment.Api.Data;

public class HospitalAppointmentDbContext : DbContext
{
    public HospitalAppointmentDbContext(DbContextOptions<HospitalAppointmentDbContext> options) : base(options) { }

    public DbSet<Role> Roles => Set<Role>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Staff> Staff => Set<Staff>();
    public DbSet<Patient> Patients => Set<Patient>();
    public DbSet<Doctor> Doctors => Set<Doctor>();
    public DbSet<Department> Departments => Set<Department>();
    public DbSet<DoctorDepartment> DoctorDepartments => Set<DoctorDepartment>();
    public DbSet<MedicalService> MedicalServices => Set<MedicalService>();
    public DbSet<DoctorScheduleSlot> DoctorScheduleSlots => Set<DoctorScheduleSlot>();
    public DbSet<Appointment> Appointments => Set<Appointment>();
    public DbSet<MedicalRecord> MedicalRecords => Set<MedicalRecord>();
    public DbSet<OrderedService> OrderedServices => Set<OrderedService>();
    public DbSet<Prescription> Prescriptions => Set<Prescription>();
    public DbSet<PrescriptionItem> PrescriptionItems => Set<PrescriptionItem>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Role>(e =>
        {
            e.ToTable("Roles");
            e.HasKey(x => x.RoleId);
            e.Property(x => x.RoleName).HasMaxLength(50).IsRequired();
            e.Property(x => x.Description).HasMaxLength(250);
            e.HasIndex(x => x.RoleName).IsUnique();
            e.HasData(
                new Role { RoleId = 1, RoleName = "Patient", Description = "Patient account" },
                new Role { RoleId = 2, RoleName = "Doctor", Description = "Doctor account" },
                new Role { RoleId = 3, RoleName = "Nurse", Description = "Nurse account" },
                new Role { RoleId = 4, RoleName = "Receptionist", Description = "Reception and appointment handling" },
                new Role { RoleId = 5, RoleName = "UserHRManager", Description = "User, staff and role administration" },
                new Role { RoleId = 6, RoleName = "MedicalServiceManager", Description = "Department and medical service administration" },
                new Role { RoleId = 7, RoleName = "SchedulingManager", Description = "Doctor schedule administration" },
                new Role { RoleId = 8, RoleName = "HospitalManager", Description = "Dashboard and reports" });
        });

        modelBuilder.Entity<User>(e =>
        {
            e.ToTable("Users"); e.HasKey(x => x.UserId);
            e.Property(x => x.Username).HasMaxLength(100).IsRequired();
            e.Property(x => x.Email).HasMaxLength(254).IsRequired();
            e.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            e.Property(x => x.Phone).HasMaxLength(30);
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired();
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(x => x.Username).IsUnique(); e.HasIndex(x => x.Email).IsUnique();
            e.HasOne(x => x.Role).WithMany(x => x.Users).HasForeignKey(x => x.RoleId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Staff>(e =>
        {
            e.ToTable("Staff"); e.HasKey(x => x.StaffId);
            e.Property(x => x.EmployeeCode).HasMaxLength(50).IsRequired(); e.Property(x => x.JobTitle).HasMaxLength(100);
            e.Property(x => x.StaffStatus).HasConversion<string>().HasMaxLength(20).HasDefaultValue(StaffStatus.Active);
            e.HasIndex(x => x.UserId).IsUnique(); e.HasIndex(x => x.EmployeeCode).IsUnique();
            e.HasOne(x => x.User).WithOne(x => x.Staff).HasForeignKey<Staff>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Patient>(e =>
        {
            e.ToTable("Patients", t => t.HasCheckConstraint("CK_Patients_Gender", "[Gender] IS NULL OR [Gender] IN (N'Male', N'Female', N'Other')"));
            e.HasKey(x => x.PatientId); e.Property(x => x.PatientCode).HasMaxLength(50).IsRequired();
            e.Property(x => x.FullName).HasMaxLength(150).IsRequired(); e.Property(x => x.Phone).HasMaxLength(30); e.Property(x => x.Gender).HasMaxLength(20); e.Property(x => x.Address).HasMaxLength(300);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()"); e.HasIndex(x => x.PatientCode).IsUnique();
            e.HasIndex(x => x.UserId).IsUnique().HasFilter("[UserId] IS NOT NULL");
            e.HasOne(x => x.User).WithOne(x => x.Patient).HasForeignKey<Patient>(x => x.UserId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Doctor>(e =>
        {
            e.ToTable("Doctors"); e.HasKey(x => x.DoctorId); e.Property(x => x.Specialty).HasMaxLength(150);
            e.Property(x => x.DoctorStatus).HasConversion<string>().HasMaxLength(20).HasDefaultValue(DoctorStatus.Active);
            e.HasIndex(x => x.StaffId).IsUnique();
            e.HasOne(x => x.Staff).WithOne(x => x.Doctor).HasForeignKey<Doctor>(x => x.StaffId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Department>(e =>
        {
            e.ToTable("Departments"); e.HasKey(x => x.DepartmentId);
            e.Property(x => x.DepartmentCode).HasMaxLength(30).IsRequired(); e.Property(x => x.DepartmentName).HasMaxLength(150).IsRequired(); e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()"); e.HasIndex(x => x.DepartmentCode).IsUnique(); e.HasIndex(x => x.DepartmentName).IsUnique();
        });

        modelBuilder.Entity<DoctorDepartment>(e =>
        {
            e.ToTable("DoctorDepartments"); e.HasKey(x => new { x.DoctorId, x.DepartmentId });
            e.Property(x => x.AssignedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(x => x.Doctor).WithMany(x => x.DepartmentAssignments).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Department).WithMany(x => x.DoctorAssignments).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.AssignedByUser).WithMany(x => x.DepartmentAssignments).HasForeignKey(x => x.AssignedBy).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<MedicalService>(e =>
        {
            e.ToTable("MedicalServices"); e.HasKey(x => x.MedicalServiceId);
            e.Property(x => x.ServiceCode).HasMaxLength(40).IsRequired(); e.Property(x => x.ServiceName).HasMaxLength(150).IsRequired(); e.Property(x => x.Description).HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()"); e.HasIndex(x => x.ServiceCode).IsUnique();
            e.HasOne(x => x.Department).WithMany(x => x.MedicalServices).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<DoctorScheduleSlot>(e =>
        {
            e.ToTable("DoctorScheduleSlots", t => t.HasCheckConstraint("CK_DoctorScheduleSlots_Time", "[EndTime] > [StartTime]")); e.HasKey(x => x.ScheduleSlotId);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()"); e.HasIndex(x => new { x.DoctorId, x.SlotDate, x.StartTime }).IsUnique();
            e.HasOne(x => x.Doctor).WithMany(x => x.ScheduleSlots).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Department).WithMany(x => x.ScheduleSlots).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.CreatedByUser).WithMany(x => x.CreatedScheduleSlots).HasForeignKey(x => x.CreatedBy).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<Appointment>(e =>
        {
            e.ToTable("Appointments", t => t.HasCheckConstraint("CK_Appointments_Status", "[Status] IN (N'Pending',N'Confirmed',N'CheckedIn',N'ReadyForConsultation',N'InConsultation',N'Completed',N'Rejected',N'Cancelled',N'NoShow')"));
            e.HasKey(x => x.AppointmentId); e.Property(x => x.AppointmentCode).HasMaxLength(40).IsRequired();
            e.Property(x => x.Status).HasConversion<string>().HasMaxLength(30).HasDefaultValue(AppointmentStatus.Pending);
            e.Property(x => x.BookingReason).HasMaxLength(500); e.Property(x => x.RejectionReason).HasMaxLength(500); e.Property(x => x.CancellationReason).HasMaxLength(500); e.Property(x => x.NoShowReason).HasMaxLength(500);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()"); e.Property(x => x.RowVersion).IsRowVersion(); e.HasIndex(x => x.AppointmentCode).IsUnique();
            e.HasIndex(x => x.ScheduleSlotId).IsUnique().HasFilter("[IsActiveBooking] = 1");
            e.HasIndex(x => new { x.DoctorId, x.Status }); e.HasIndex(x => new { x.PatientId, x.CreatedAt }).IsDescending(false, true);
            e.HasOne(x => x.Patient).WithMany(x => x.Appointments).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Doctor).WithMany(x => x.Appointments).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Department).WithMany(x => x.Appointments).HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ScheduleSlot).WithMany(x => x.Appointments).HasForeignKey(x => x.ScheduleSlotId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.ConfirmedByUser).WithMany(x => x.ConfirmedAppointments).HasForeignKey(x => x.ConfirmedBy).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.CheckedInByUser).WithMany(x => x.CheckedInAppointments).HasForeignKey(x => x.CheckedInBy).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.CompletedByUser).WithMany(x => x.CompletedAppointments).HasForeignKey(x => x.CompletedBy).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.CancelledByUser).WithMany(x => x.CancelledAppointments).HasForeignKey(x => x.CancelledBy).OnDelete(DeleteBehavior.NoAction);
            e.HasOne(x => x.NoShowByUser).WithMany(x => x.NoShowAppointments).HasForeignKey(x => x.NoShowBy).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<MedicalRecord>(e =>
        {
            e.ToTable("MedicalRecords", t => t.HasCheckConstraint("CK_MedicalRecords_Status", "[RecordStatus] IN (N'Draft', N'Final')")); e.HasKey(x => x.MedicalRecordId);
            e.Property(x => x.RecordStatus).HasConversion<string>().HasMaxLength(20).HasDefaultValue(RecordStatus.Draft);
            e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()"); e.HasIndex(x => x.AppointmentId).IsUnique(); e.HasIndex(x => new { x.PatientId, x.CreatedAt }).IsDescending(false, true);
            e.HasOne(x => x.Appointment).WithOne(x => x.MedicalRecord).HasForeignKey<MedicalRecord>(x => x.AppointmentId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Patient).WithMany(x => x.MedicalRecords).HasForeignKey(x => x.PatientId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.Doctor).WithMany(x => x.MedicalRecords).HasForeignKey(x => x.DoctorId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.FinalizedByUser).WithMany(x => x.FinalizedRecords).HasForeignKey(x => x.FinalizedBy).OnDelete(DeleteBehavior.NoAction);
        });

        modelBuilder.Entity<OrderedService>(e =>
        {
            e.ToTable("OrderedServices", t => t.HasCheckConstraint("CK_OrderedServices_Status", "[ServiceStatus] IN (N'Ordered',N'InProgress',N'Completed',N'Cancelled')")); e.HasKey(x => x.OrderedServiceId);
            e.Property(x => x.Notes).HasMaxLength(500); e.Property(x => x.ServiceStatus).HasConversion<string>().HasMaxLength(30).HasDefaultValue(ServiceStatus.Ordered); e.Property(x => x.OrderedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasOne(x => x.MedicalRecord).WithMany(x => x.OrderedServices).HasForeignKey(x => x.MedicalRecordId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.MedicalService).WithMany(x => x.OrderedServices).HasForeignKey(x => x.MedicalServiceId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(x => x.OrderedByDoctor).WithMany(x => x.OrderedServices).HasForeignKey(x => x.OrderedByDoctorId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Prescription>(e =>
        {
            e.ToTable("Prescriptions"); e.HasKey(x => x.PrescriptionId); e.Property(x => x.Notes).HasMaxLength(1000); e.Property(x => x.PrescribedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(x => x.MedicalRecordId).IsUnique();
            e.HasOne(x => x.MedicalRecord).WithOne(x => x.Prescription).HasForeignKey<Prescription>(x => x.MedicalRecordId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<PrescriptionItem>(e =>
        {
            e.ToTable("PrescriptionItems", t => t.HasCheckConstraint("CK_PrescriptionItems_Quantity", "[Quantity] IS NULL OR [Quantity] > 0")); e.HasKey(x => x.PrescriptionItemId);
            e.Property(x => x.MedicationName).HasMaxLength(200).IsRequired(); e.Property(x => x.Dosage).HasMaxLength(100); e.Property(x => x.Route).HasMaxLength(100); e.Property(x => x.Frequency).HasMaxLength(200); e.Property(x => x.Duration).HasMaxLength(100); e.Property(x => x.Instructions).HasMaxLength(500); e.Property(x => x.Quantity).HasColumnType("decimal(10,2)");
            e.HasOne(x => x.Prescription).WithMany(x => x.Items).HasForeignKey(x => x.PrescriptionId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AuditLog>(e =>
        {
            e.ToTable("AuditLogs"); e.HasKey(x => x.AuditLogId); e.Property(x => x.Action).HasMaxLength(100).IsRequired(); e.Property(x => x.EntityName).HasMaxLength(100).IsRequired(); e.Property(x => x.EntityId).HasMaxLength(100); e.Property(x => x.Details).HasMaxLength(1000); e.Property(x => x.CreatedAt).HasDefaultValueSql("SYSUTCDATETIME()");
            e.HasIndex(x => x.CreatedAt).IsDescending(); e.HasOne(x => x.User).WithMany(x => x.AuditLogs).HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.SetNull);
        });
    }
}
