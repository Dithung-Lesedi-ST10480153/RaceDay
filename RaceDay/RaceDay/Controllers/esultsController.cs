using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.DTOs;
using RaceDay.Data;
using RaceDay.Data.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class ResultsController(
    ApplicationDbContext db) : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(
            ClaimTypes.NameIdentifier)!
        );

    /// <summary>
    /// Record or update a result for an enrolment
    /// belonging to an event owned by the organiser.
    /// </summary>
    [HttpPost("enrolments/{enrolmentId:int}/result")]
    [Authorize(Roles = AppRoles.Organiser)]
    public async Task<IActionResult> Record(
        int enrolmentId,
        ResultCreateRequest request)
    {
        if (request.FinishTime <= TimeSpan.Zero ||
            request.FinishTime.TotalDays >= 1)
        {
            return BadRequest(new
            {
                message = "FinishTime must be positive and under 24 hours."
            });
        }

        var enrolment = await db.Enrolments
            .Include(e => e.Event)
            .Include(e => e.Result)
            .SingleOrDefaultAsync(
                e => e.EnrolmentId == enrolmentId);

        if (enrolment is null)
            return NotFound();

        if (enrolment.Event!.OrganiserId != CurrentUserId)
            return Forbid();

        if (enrolment.Result is null)
        {
            enrolment.Result = new RaceResult
            {
                FinishTime = request.FinishTime,
                FinishingPosition = request.FinishingPosition,
                RecordedUtc = DateTime.UtcNow
            };
        }
        else
        {
            enrolment.Result.FinishTime = request.FinishTime;
            enrolment.Result.FinishingPosition =
                request.FinishingPosition;
            enrolment.Result.RecordedUtc = DateTime.UtcNow;
        }

        await db.SaveChangesAsync();

        return Ok(new
        {
            enrolment.EnrolmentId,
            enrolment.Result.FinishTime,
            enrolment.Result.FinishingPosition,
            enrolment.Result.RecordedUtc
        });
    }

    /// <summary>Get results belonging to the current participant.</summary>
    [HttpGet("results/me")]
    [Authorize(Roles = AppRoles.Participant)]
    public async Task<IActionResult> Mine()
    {
        var results = await db.Results
            .AsNoTracking()
            .Where(r =>
                r.Enrolment!.ParticipantId == CurrentUserId)
            .Select(r => new
            {
                r.ResultId,
                r.FinishTime,
                r.FinishingPosition,
                r.RecordedUtc,
                r.Enrolment!.EventId,
                EventName = r.Enrolment.Event!.Name,
                r.Enrolment.CategoryId,
                CategoryName = r.Enrolment.Category!.Name
            })
            .ToListAsync();

        return Ok(results);
    }
}