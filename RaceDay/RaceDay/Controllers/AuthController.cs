using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using RaceDay.API.DTOs;
using RaceDay.Data;
using RaceDay.Data.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ApplicationDbContext db)
    : ControllerBase
{
    private readonly PasswordHasher<User> _hasher = new();

    /// <summary>
    /// Register a new Organiser or Participant.
    /// The password is hashed before it is stored.
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), 201)]
    [ProducesResponseType(400)]
    [ProducesResponseType(409)]
    public async Task<IActionResult> Register(RegisterRequest request)
    {
        var role = request.Role.Trim();

        if (role != AppRoles.Organiser &&
            role != AppRoles.Participant)
        {
            return BadRequest(new
            {
                message = "Role must be Organiser or Participant."
            });
        }

        var email = request.Email.Trim().ToLowerInvariant();

        if (await db.Users.AnyAsync(u => u.Email == email))
        {
            return Conflict(new
            {
                message = "An account with this email already exists."
            });
        }

        var user = new User
        {
            FullName = request.FullName.Trim(),
            Email = email,
            Role = role
        };

        user.PasswordHash =
            _hasher.HashPassword(user, request.Password);

        db.Users.Add(user);
        await db.SaveChangesAsync();

        var response = new UserResponse(
            user.UserId,
            user.FullName,
            user.Email,
            user.Role
        );

        return Created("/api/users/me", response);
    }

    /// <summary>
    /// Validate credentials and issue an authentication cookie.
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(UserResponse), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await db.Users.SingleOrDefaultAsync(
            u => u.Email == email);

        if (user is null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var verification = _hasher.VerifyHashedPassword(
            user,
            user.PasswordHash,
            request.Password
        );

        if (verification == PasswordVerificationResult.Failed)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password."
            });
        }

        var claims = new List<Claim>
        {
            new(
                ClaimTypes.NameIdentifier,
                user.UserId.ToString()
            ),
            new(ClaimTypes.Name, user.FullName),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role)
        };

        var identity = new ClaimsIdentity(
            claims,
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        var principal = new ClaimsPrincipal(identity);

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            new AuthenticationProperties
            {
                IsPersistent = true,
                AllowRefresh = true,
                ExpiresUtc = DateTimeOffset.UtcNow.AddHours(2)
            }
        );

        return Ok(new UserResponse(
            user.UserId,
            user.FullName,
            user.Email,
            user.Role
        ));
    }

    /// <summary>Sign out the current user.</summary>
    [HttpPost("logout")]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(
            CookieAuthenticationDefaults.AuthenticationScheme
        );

        return NoContent();
    }
}