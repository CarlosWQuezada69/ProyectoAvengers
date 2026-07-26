using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Logging;
using ProyectoAvengers.Application.Interfaces;

namespace ProyectoAvengers.Infrastructure.Services;

public class SmtpEmailSender : IEmailSender
{
    private readonly string _host;
    private readonly int _port;
    private readonly string _user;
    private readonly string _password;
    private readonly string _fromEmail;
    private readonly ILogger<SmtpEmailSender> _logger;

    public SmtpEmailSender(string host, int port, string user, string password, string fromEmail, ILogger<SmtpEmailSender> logger)
    {
        _host = host;
        _port = port;
        _user = user;
        _password = password;
        _fromEmail = fromEmail;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
    {
        try
        {
            using var client = new SmtpClient(_host, _port)
            {
                Credentials = new NetworkCredential(_user, _password),
                EnableSsl = _port == 587
            };

            using var message = new MailMessage(_fromEmail, to, subject, body)
            {
                IsBodyHtml = body.Contains('<')
            };

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Email sent to {To} with subject {Subject}", to, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {To}", to);
        }
    }
}