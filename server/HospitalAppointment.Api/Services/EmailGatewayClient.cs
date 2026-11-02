using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Options;

namespace HospitalAppointment.Api.Services;

public sealed class EmailGatewayClient : IEmailSender
{
    private readonly EmailSettings _settings;

    public EmailGatewayClient(IOptions<EmailSettings> options)
    {
        _settings = options.Value;
    }

    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        using var message = new MailMessage
        {
            From = new MailAddress(_settings.FromEmail, _settings.FromName, Encoding.UTF8),
            Subject = subject,
            SubjectEncoding = Encoding.UTF8,
            Body = htmlBody,
            BodyEncoding = Encoding.UTF8,
            IsBodyHtml = true
        };
        message.To.Add(new MailAddress(toEmail, toName, Encoding.UTF8));

        using var client = new SmtpClient(_settings.Host, _settings.Port)
        {
            EnableSsl = _settings.EnableSsl,
            Credentials = new NetworkCredential(_settings.Username, _settings.Password),
            Timeout = _settings.TimeoutMs
        };

        await client.SendMailAsync(message);
    }
}