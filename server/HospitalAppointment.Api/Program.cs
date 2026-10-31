using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Extensions;
using HospitalAppointment.Api.Middlewares;
using HospitalAppointment.Api.Services.Implementations;
using HospitalAppointment.Api.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using HospitalAppointment.Api.Repositories;
using HospitalAppointment.Api.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Security.Claims;
using System.Text;


var builder = WebApplication.CreateBuilder(args);

// 1. Database
builder.Services.AddDbContext<HospitalAppointmentDbContext>(
    options => options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Controllers
builder.Services.AddControllers();

// 3. Swagger (nút Authorize/JWT được cấu hình trong AddApplicationServices)
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// 4. Repositories
builder.Services.AddScoped<IAuthRepository, AuthRepository>();
builder.Services.AddScoped<INurseRepository, NurseRepository>();

// 5. Services
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<INurseService, NurseService>();

builder.Services.AddScoped<IDepartmentRepository, DepartmentRepository>();
builder.Services.AddScoped<IDepartmentService, DepartmentService>();
// AppointmentRepository hiện thực 2 interface cùng tên ở 2 namespace -> đăng ký cả hai
builder.Services.AddScoped<HospitalAppointment.Api.Repositories.IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<HospitalAppointment.Api.Repositories.Interfaces.IAppointmentRepository, AppointmentRepository>();
builder.Services.AddScoped<IAppointmentReceptionService, AppointmentReceptionService>();

builder.Services.AddScoped<
    IMedicalServiceRepository,
    MedicalServiceRepository>();

builder.Services.AddScoped<
    IMedicalServiceService,
    MedicalServiceService>();

// JWT + Authorization + Swagger Authorize được cấu hình trong AddApplicationServices

builder.Services.AddCors(options =>
{
    options.AddPolicy("BlazorClient", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// JWT, CORS (policy "Client"), Swagger Authorize, và DI các service/repository còn lại
builder.Services.AddApplicationServices(builder.Configuration);

var app = builder.Build();

app.UseMiddleware<ExceptionHandlingMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseCors("BlazorClient");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

await app.SeedDevDataAsync();

app.Run();