using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoIdentityUser = MongoDB.AspNet.Identity.IdentityUser;

namespace SampleWebApp.Controllers;

[ApiController]
[Route("api/users/{userId}/[controller]")]
public class ClaimsController : ControllerBase
{
    private readonly UserManager<MongoIdentityUser> _userManager;

    public ClaimsController(UserManager<MongoIdentityUser> userManager)
    {
        _userManager = userManager;
    }

    /// <summary>
    /// Get all claims for a user.
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetClaims(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "User not found" });

        var claims = await _userManager.GetClaimsAsync(user);
        return Ok(claims.Select(c => new { c.Type, c.Value }));
    }

    /// <summary>
    /// Add a claim to a user.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> AddClaim(string userId, [FromBody] ClaimRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "User not found" });

        var claim = new Claim(request.Type, request.Value);
        var result = await _userManager.AddClaimAsync(user, claim);

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = $"Claim '{request.Type}' added successfully" });
    }

    /// <summary>
    /// Remove a claim from a user.
    /// </summary>
    [HttpDelete]
    public async Task<IActionResult> RemoveClaim(string userId, [FromQuery] string type, [FromQuery] string value)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "User not found" });

        var claim = new Claim(type, value);
        var result = await _userManager.RemoveClaimAsync(user, claim);

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = "Claim removed successfully" });
    }
}

public record ClaimRequest(string Type, string Value);
