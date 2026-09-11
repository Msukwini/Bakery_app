using BakeryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BakeryApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class NotificationsController : ControllerBase
{
    private readonly IEmailService _emailService;

    public NotificationsController(IEmailService emailService)
    {
        _emailService = emailService;
    }

    [HttpGet]
    public async Task<IActionResult> GetRecent([FromQuery] int limit = 50)
    {
        var notifications = await _emailService.GetRecentAsync(limit);

        var response = notifications.Select(n => new
        {
            n.Id,
            n.Type,
            n.RecipientEmail,
            n.Subject,
            n.Sent,
            n.ErrorMessage,
            n.CreatedAt,
            n.SentAt
        });

        return Ok(response);
    }
}
