namespace TaskManager.Application.Interfaces;

public interface IEmailService
{
    public Task SendMailAsync(string to, string subject, string htmlMessage);
}