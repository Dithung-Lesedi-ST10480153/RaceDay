using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.DTOs;
using RaceDay.Data;
using RaceDay.Data.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/enrolments")]
[Authorize]
public sealed class EnrolmentsController(
    ApplicationDbContext db) : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(
            ClaimTypes.NameIdentifier)!
        );

    /// <summary>Enrol the current participant in an event.</summary>
    [HttpPost]
    [Authorize(Roles = AppRoles.Participant)]
    public async Task<IActionResult> Create(
        EnrolmentCreateRequest request)
    {
        var eventExists = await db.Events
            .AnyAsync(e => e.EventId == request.EventId);

        if (!eventExists)
            return NotFound(new { message = "Event not found." });

        var category = await db.Categories
            .SingleOrDefaultAsync(c =>
                c.CategoryId == request.CategoryId &&
                c.EventId == request.EventId);

        if (category is null)
        {
            return BadRequest(new
            {
                message = "The category does not belong to this event."
            });
        }

        var alreadyEnrolled = await db.Enrolments
            .AnyAsync(e =>
                e.ParticipantId == CurrentUserId &&
                e.EventId == request.EventId);

        if (alreadyEnrolled)
        {
            return Conflict(new
            {
                message = "You are already enrolled in this event."
            });
        }

        var enrolment = new Enrolment
        {
            ParticipantId = CurrentUserId,
            EventId = request.EventId,
            CategoryId = request.CategoryId,
            EnrolmentDateUtc = DateTime.UtcNow
        };

        db.Enrolments.Add(enrolment);

        await db.SaveChangesAsync();

        return Created(
            $"/api/enrolments/{enrolment.EnrolmentId}",
            new
            {
                enrolment.EnrolmentId,
                enrolment.ParticipantId,
                enrolment.EventId,
                enrolment.CategoryId,
                enrolment.EnrolmentDateUtc
            }
        );
    }

    /// <summary>
    /// Organisers can view enrolments for their own events only.
    /// </summary>
    [HttpGet("/api/events/{eventId:int}/enrolments")]
    [Authorize(Roles = AppRoles.Organiser)]
    public async Task<IActionResult> ForEvent(int eventId)
    {
        var item = await db.Events.FindAsync(eventId);

        if (item is null)
            return NotFound();

        if (item.OrganiserId != CurrentUserId)
            return Forbid();

        var enrolments = await db.Enrolments
            .AsNoTracking()
            .Where(e => e.EventId == eventId)
            .Select(e => new
            {
                e.EnrolmentId,
                e.ParticipantId,
                ParticipantName = e.Participant!.FullName,
                e.EventId,
                e.CategoryId,
                CategoryName = e.Category!.Name,
                e.EnrolmentDateUtc
            })
            .ToListAsync();

        return Ok(enrolments);
    }
}
