using BakeryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BakeryApp.Api.Controllers;

public class ForgotPasswordRequest
{
    public string Identifier { get; set; } = string.Empty; // email OR phone
}

public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

[ApiController]
[Route("api/auth")]
public class ForgotPasswordController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly INotificationService _notificationService;

    public ForgotPasswordController(IAuthService authService, INotificationService notificationService)
    {
        _authService = authService;
        _notificationService = notificationService;
    }

    /// <summary>
    /// Request a password reset. Always returns success (doesn't reveal if account exists).
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Identifier))
            return BadRequest(new { error = "Email or phone number is required." });

        try
        {
            var person = await _authService.RequestPasswordResetAsync(req.Identifier);

            if (person != null)
            {
                // Build reset link
                var frontendUrl = "https://ndlovufreshgoods.duckdns.org";
                var resetLink = $"{frontendUrl}/reset-password?token={person.PasswordResetToken}";

                await _notificationService.SendPasswordResetEmailAsync(
                    person.Email,
                    person.FirstName,
                    resetLink
                );
            }

            // Always return success (security: don't reveal if account exists)
            return Ok(new { message = "If an account exists with that email or phone, you'll receive a reset link shortly." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    /// <summary>
    /// Verify a reset token is valid (used by the reset page on load).
    /// </summary>
    [HttpGet("verify-reset-token/{token}")]
    public async Task<IActionResult> VerifyResetToken(string token)
    {
        var valid = await _authService.VerifyResetTokenAsync(token);
        return Ok(new { valid });
    }

    /// <summary>
    /// Reset password using the token from the email.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Token))
            return BadRequest(new { error = "Token is required." });
        if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 6)
            return BadRequest(new { error = "Password must be at least 6 characters." });

        try
        {
            await _authService.ResetPasswordAsync(req.Token, req.NewPassword);
            return Ok(new { message = "Password reset successfully. You can now log in." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
