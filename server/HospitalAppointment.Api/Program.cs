using HospitalAppointment.Api.Data;
using HospitalAppointment.Api.Extensions;
using HospitalAppointment.Api.Middlewares;
using HospitalAppointment.Api.Services.Reception;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<HospitalAppointmentDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddScoped<IReceptionService, ReceptionService>();
builder.Services.AddScoped<IHospitalClock, SystemHospitalClock>();

var app = builder.Build();
app.UseMiddleware<ExceptionHandlingMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

if (!app.Environment.IsDevelopment())
{
    app.UseHttpsRedirection();
}
app.UseCors(ServiceCollectionExtensions.CorsPolicy);
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

await app.SeedDevDataAsync();

app.Run();
