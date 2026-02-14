using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using MongoIdentityRole = MongoDB.AspNet.Identity.IdentityRole;

namespace SampleWebApp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class RolesController : ControllerBase
{
    private readonly RoleManager<MongoIdentityRole> _roleManager;

    public RolesController(RoleManager<MongoIdentityRole> roleManager)
    {
        _roleManager = roleManager;
    }

    /// <summary>
    /// Get all roles.
    /// </summary>
    [HttpGet]
    public IActionResult GetAllRoles()
    {
        var roles = _roleManager.Roles.ToList();
        return Ok(roles.Select(r => new { r.Id, r.Name }));
    }

    /// <summary>
    /// Create a new role.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest(new { Message = "Role name is required" });

        var role = new MongoIdentityRole(request.Name);
        var result = await _roleManager.CreateAsync(role);

        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = "Role created successfully", RoleId = role.Id, RoleName = role.Name });
    }

    /// <summary>
    /// Delete a role.
    /// </summary>
    [HttpDelete("{roleId}")]
    public async Task<IActionResult> DeleteRole(string roleId)
    {
        var role = await _roleManager.FindByIdAsync(roleId);
        if (role == null)
            return NotFound(new { Message = "Role not found" });

        var result = await _roleManager.DeleteAsync(role);
        if (!result.Succeeded)
        {
            return BadRequest(new { Errors = result.Errors.Select(e => e.Description) });
        }

        return Ok(new { Message = "Role deleted successfully" });
    }
}

public record CreateRoleRequest(string Name);
