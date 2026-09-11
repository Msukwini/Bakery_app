using BakeryApp.Core.Entities;
using BakeryApp.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace BakeryApp.Infrastructure.Services;

public interface IEmailService
{
    Task SendAsync(string toEmail, string subject, string htmlBody, string type);
    Task<List<Notification>> GetRecentAsync(int limit = 50);
}

public class EmailService : IEmailService
{
    private readonly BakeryDbContext _context;
    private readonly string? _apiKey;
    private readonly string _fromEmail;
    private readonly string _fromName;

    public EmailService(BakeryDbContext context, IConfiguration config)
    {
        _context = context;
        _apiKey = config["SendGrid:ApiKey"];
        _fromEmail = config["SendGrid:FromEmail"] ?? "lwzimsukwini@gmail.com";
        _fromName = config["SendGrid:FromName"] ?? "Ndlovu Bakery";
    }

    public async Task SendAsync(string toEmail, string subject, string htmlBody, string type)
    {
        var notification = new Notification
        {
            Type = type,
            RecipientEmail = toEmail,
            Subject = subject,
            Body = htmlBody
        };

        if (string.IsNullOrEmpty(_apiKey))
        {
            notification.ErrorMessage = "SendGrid API key not configured";
            _context.Notifications.Add(notification);
            await _context.SaveChangesAsync();
            Console.WriteLine($"[EMAIL-SKIP] {subject} -> {toEmail}");
            return;
        }

        try
        {
            var client = new SendGridClient(_apiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, null, htmlBody);
            var response = await client.SendEmailAsync(msg);

            if (response.IsSuccessStatusCode)
            {
                notification.Sent = true;
                notification.SentAt = DateTime.UtcNow;
            }
            else
            {
                var body = await response.Body.ReadAsStringAsync();
                notification.ErrorMessage = $"{response.StatusCode}: {body}";
            }
        }
        catch (Exception ex)
        {
            notification.ErrorMessage = ex.Message;
        }

        _context.Notifications.Add(notification);
        await _context.SaveChangesAsync();
    }

    public async Task<List<Notification>> GetRecentAsync(int limit = 50)
    {
        return await _context.Notifications
            .OrderByDescending(n => n.CreatedAt)
            .Take(limit)
            .ToListAsync();
    }
}
