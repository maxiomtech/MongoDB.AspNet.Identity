using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoIdentityUser = MongoDB.AspNet.Identity.IdentityUser;
using MongoIdentityRole = MongoDB.AspNet.Identity.IdentityRole;

namespace SampleWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<MongoIdentityUser> _userManager;
    private readonly SignInManager<MongoIdentityUser> _signInManager;
    private readonly RoleManager<MongoIdentityRole> _roleManager;

    public AuthController(
        UserManager<MongoIdentityUser> userManager,
        SignInManager<MongoIdentityUser> signInManager,
        RoleManager<MongoIdentityRole> roleManager)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _roleManager = roleManager;
    }

    /// <summary>
    /// Register a new user.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = new MongoIdentityUser(request.UserName)
        {
            Email = request.Email
        };

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = "User registered successfully", UserId = user.Id });
    }

    /// <summary>
    /// Login with username and password.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var user = await _userManager.FindByNameAsync(request.UserName);
        if (user == null)
        {
            return Unauthorized(new { Message = "Invalid username or password" });
        }

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
        {
            if (result.IsLockedOut)
                return Unauthorized(new { Message = "Account is locked out" });

            return Unauthorized(new { Message = "Invalid username or password" });
        }

        return Ok(new { Message = "Login successful", UserId = user.Id, UserName = user.UserName });
    }

    /// <summary>
    /// Get current user info.
    /// </summary>
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetCurrentUser()
    {
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
            return NotFound();

        var roles = await _userManager.GetRolesAsync(user);
        var claims = await _userManager.GetClaimsAsync(user);

        return Ok(new
        {
            user.Id,
            user.UserName,
            user.Email,
            user.EmailConfirmed,
            user.PhoneNumber,
            user.PhoneNumberConfirmed,
            user.TwoFactorEnabled,
            Roles = roles,
            Claims = claims.Select(c => new { c.Type, c.Value })
        });
    }

    /// <summary>
    /// Add a role to a user.
    /// </summary>
    [HttpPost("users/{userId}/roles")]
    public async Task<IActionResult> AddToRole(string userId, [FromBody] RoleRequest request)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "User not found" });

        // Validate that the role exists
        var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
        if (!roleExists)
            return BadRequest(new { Message = $"Role '{request.RoleName}' does not exist. Create it first." });

        var result = await _userManager.AddToRoleAsync(user, request.RoleName);
        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = $"User added to role '{request.RoleName}'" });
    }

    /// <summary>
    /// Get all users.
    /// </summary>
    [HttpGet("users")]
    public IActionResult GetAllUsers()
    {
        var users = _userManager.Users.ToList();
        return Ok(users.Select(u => new
        {
            u.Id,
            u.UserName,
            u.Email,
            u.EmailConfirmed,
            u.Roles
        }));
    }

    /// <summary>
    /// Delete a user.
    /// </summary>
    [HttpDelete("users/{userId}")]
    public async Task<IActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound(new { Message = "User not found" });

        var result = await _userManager.DeleteAsync(user);
        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = "User deleted successfully" });
    }

    /// <summary>
    /// Request a password reset token.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
        {
            // Don't reveal that user doesn't exist
            return Ok(new { Message = "If the email exists, a reset token has been generated." });
        }

        var token = await _userManager.GeneratePasswordResetTokenAsync(user);

        // In production, you'd send this via email. For demo, we return it.
        return Ok(new {
            Message = "Password reset token generated",
            Token = token,
            UserId = user.Id,
            Note = "In production, this token would be sent via email"
        });
    }

    /// <summary>
    /// Reset password using token.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var user = await _userManager.FindByIdAsync(request.UserId);
        if (user == null)
            return BadRequest(new { Message = "Invalid request" });

        var result = await _userManager.ResetPasswordAsync(user, request.Token, request.NewPassword);

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = "Password has been reset successfully" });
    }
}

public record RegisterRequest(string UserName, string Email, string Password);
public record LoginRequest(string UserName, string Password);
public record RoleRequest(string RoleName);
public record ForgotPasswordRequest(string Email);
public record ResetPasswordRequest(string UserId, string Token, string NewPassword);
