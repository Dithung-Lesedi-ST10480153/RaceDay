using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.DTOs;
using RaceDay.Data;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/users")]
[Authorize]
public sealed class UsersController(ApplicationDbContext db)
    : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(
            ClaimTypes.NameIdentifier)!
        );

    /// <summary>Get the current user's profile.</summary>
    [HttpGet("me")]
    public async Task<IActionResult> Me()
    {
        var user = await db.Users.FindAsync(CurrentUserId);

        if (user is null)
            return NotFound();

        return Ok(new UserResponse(
            user.UserId,
            user.FullName,
            user.Email,
            user.Role
        ));
    }

    /// <summary>Update the current user's profile name.</summary>
    [HttpPut("me")]
    public async Task<IActionResult> Update(
        ProfileUpdateRequest request)
    {
        var user = await db.Users.FindAsync(CurrentUserId);

        if (user is null)
            return NotFound();

        user.FullName = request.FullName.Trim();

        await db.SaveChangesAsync();

        return Ok(new UserResponse(
            user.UserId,
            user.FullName,
            user.Email,
            user.Role
        ));
    }
}