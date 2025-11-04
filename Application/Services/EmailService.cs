using MailKit.Net.Smtp;
using MimeKit;
using TaskManager.Application.Interfaces;

namespace TaskManager.Application.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;
    
    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }
    
    public async Task SendMailAsync(string to, string subject, string htmlMessage)
    {
        _logger.LogInformation($"Попытка отправить письмо на почту {to}");

        if (string.IsNullOrEmpty(to) || string.IsNullOrEmpty(subject) || string.IsNullOrEmpty(htmlMessage))
        {
            _logger.LogError("При отправке сообщения на почту на вход методы были переданны null значения");

            throw new ArgumentNullException();
        }
        
        var email = new MimeMessage();
        email.From.Add(MailboxAddress.Parse(_config["Email:From"]));
        email.To.Add(MailboxAddress.Parse(to));
        email.Subject = subject;

        var builder = new BodyBuilder { HtmlBody = htmlMessage };
        email.Body = builder.ToMessageBody();

        using var smtp = new SmtpClient();
        await smtp.ConnectAsync(_config["Email:SmtpServer"], int.Parse(_config["Email:Port"]), true);
        await smtp.AuthenticateAsync(_config["Email:Username"], _config["Email:Password"]);
        await smtp.SendAsync(email);
        await smtp.DisconnectAsync(true);
    }
}