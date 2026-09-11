using BakeryApp.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace BakeryApp.Api.Controllers;

public class SetupPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

[ApiController]
[Route("api/auth")]
public class SetupController : ControllerBase
{
    private readonly IAuthService _authService;

    public SetupController(IAuthService authService)
    {
        _authService = authService;
    }

    [HttpGet("verify-setup-token/{token}")]
    public async Task<IActionResult> VerifyToken(string token)
    {
        var valid = await _authService.VerifySetupTokenAsync(token);
        return Ok(new { valid });
    }

    [HttpPost("setup-password")]
    public async Task<IActionResult> SetupPassword([FromBody] SetupPasswordRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Token))
            return BadRequest(new { error = "Token is required." });
        if (string.IsNullOrWhiteSpace(req.NewPassword) || req.NewPassword.Length < 6)
            return BadRequest(new { error = "Password must be at least 6 characters." });

        try
        {
            await _authService.SetPasswordAsync(req.Token, req.NewPassword);
            return Ok(new { message = "Password set successfully. You can now log in." });
        }
        catch (Exception ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }
}
