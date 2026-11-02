using System.Net;
using System.Text;
using HospitalAppointment.Api.Models.Entities;
using Microsoft.Extensions.Options;

namespace HospitalAppointment.Api.Services;

public sealed class NotificationService : INotificationService
{
    private readonly IEmailSender _emailSender;
    private readonly EmailSettings _settings;
    private readonly ILogger<NotificationService> _logger;

    public NotificationService(
        IEmailSender emailSender,
        IOptions<EmailSettings> options,
        ILogger<NotificationService> logger)
    {
        _emailSender = emailSender;
        _settings = options.Value;
        _logger = logger;
    }

    public Task SendAppointmentConfirmedAsync(Appointment appointment)
        => SendAsync(
            appointment,
            type: "APPOINTMENT_CONFIRMED",
            subject: $"[Bệnh viện] Lịch hẹn {appointment.AppointmentCode} đã được xác nhận",
            heading: "Lịch hẹn của bạn đã được xác nhận",
            message: "Lễ tân đã xác nhận lịch hẹn của bạn. Vui lòng đến đúng giờ và mang theo giấy tờ tùy thân. " +
                     "Nếu cần hủy, bạn có thể hủy lịch trước giờ khám ít nhất 24 giờ.");

    public Task SendAppointmentCancelledAsync(Appointment appointment)
        => SendAsync(
            appointment,
            type: "APPOINTMENT_CANCELLED",
            subject: $"[Bệnh viện] Lịch hẹn {appointment.AppointmentCode} đã được hủy",
            heading: "Lịch hẹn của bạn đã được hủy",
            message: "Bạn đã hủy lịch hẹn dưới đây thành công. Khung giờ đã được giải phóng. " +
                     "Bạn có thể đặt lịch mới bất cứ lúc nào.");

    public Task SendAppointmentNoShowAsync(Appointment appointment)
        => SendAsync(
            appointment,
            type: "APPOINTMENT_NO_SHOW",
            subject: $"[Bệnh viện] Lịch hẹn {appointment.AppointmentCode} - ghi nhận vắng mặt",
            heading: "Bạn đã không đến khám theo lịch hẹn",
            message: "Bạn không có mặt theo lịch hẹn dưới đây nên lịch hẹn được ghi nhận là vắng mặt (No-show) " +
                     "và khung giờ đã được giải phóng. Nếu vẫn cần khám, vui lòng đặt lịch mới. " +
                     "Nếu có nhầm lẫn, vui lòng liên hệ quầy lễ tân.");

    private async Task SendAsync(Appointment appointment, string type, string subject, string heading, string message)
    {
        // Lỗi gửi mail không được làm hỏng thao tác đã lưu -> bắt hết, chỉ ghi log
        try
        {
            if (!_settings.Enabled)
            {
                _logger.LogInformation("Email disabled, skip {Type} for appointment {Code}.",
                    type, appointment.AppointmentCode);
                return;
            }

            var email = appointment.Patient?.User?.Email;
            if (string.IsNullOrWhiteSpace(email))
            {
                _logger.LogWarning("Patient has no email, skip {Type} for appointment {Code}.",
                    type, appointment.AppointmentCode);
                return;
            }

            var body = BuildBody(appointment, heading, message);
            await _emailSender.SendAsync(email, appointment.Patient!.FullName, subject, body);

            _logger.LogInformation("Sent {Type} email for appointment {Code}.",
                type, appointment.AppointmentCode);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send {Type} email for appointment {Code}.",
                type, appointment.AppointmentCode);
        }
    }

    private static string BuildBody(Appointment a, string heading, string message)
    {
        static string E(string? value) => WebUtility.HtmlEncode(value ?? "—");

        var slot = a.ScheduleSlot;
        var date = slot.SlotDate.ToString("dd/MM/yyyy");
        var time = $"{slot.StartTime:HH:mm} - {slot.EndTime:HH:mm}";
        var doctor = a.Doctor?.Staff?.User?.FullName;
        var department = a.Department?.DepartmentName;

        var sb = new StringBuilder();
        sb.Append("<div style=\"font-family:Arial,Helvetica,sans-serif;max-width:560px;margin:0 auto;color:#1f2937\">");
        sb.Append($"<h2 style=\"color:#0b6bcb\">{E(heading)}</h2>");
        sb.Append($"<p>Xin chào <b>{E(a.Patient.FullName)}</b>,</p>");
        sb.Append($"<p>{E(message)}</p>");
        sb.Append("<table style=\"border-collapse:collapse;width:100%\">");
        AppendRow(sb, "Mã lịch hẹn", E(a.AppointmentCode));
        AppendRow(sb, "Khoa", E(department));
        AppendRow(sb, "Bác sĩ", E(doctor));
        AppendRow(sb, "Ngày khám", E(date));
        AppendRow(sb, "Giờ khám", E(time));
        sb.Append("</table>");
        sb.Append("<p style=\"color:#6b7280;font-size:12px;margin-top:24px\">" +
                  "Đây là email tự động, vui lòng không trả lời email này.</p>");
        sb.Append("</div>");
        return sb.ToString();
    }

    private static void AppendRow(StringBuilder sb, string label, string encodedValue)
    {
        sb.Append("<tr>");
        sb.Append($"<td style=\"padding:6px 8px;border:1px solid #e5e7eb;background:#f9fafb;width:35%\">{label}</td>");
        sb.Append($"<td style=\"padding:6px 8px;border:1px solid #e5e7eb\">{encodedValue}</td>");
        sb.Append("</tr>");
    }
}