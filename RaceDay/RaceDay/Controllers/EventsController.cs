using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RaceDay.API.DTOs;
using RaceDay.Data;
using RaceDay.Data.Models;

namespace RaceDay.API.Controllers;

[ApiController]
[Route("api/events")]
public sealed class EventsController(ApplicationDbContext db)
    : ControllerBase
{
    private int CurrentUserId =>
        int.Parse(User.FindFirstValue(
            ClaimTypes.NameIdentifier)!
        );

    private static bool ValidEventType(string type)
    {
        return new[] { "run", "walk", "cycle" }
            .Contains(type.Trim().ToLowerInvariant());
    }

    /// <summary>List events visible to all users.</summary>
    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> List()
    {
        var events = await db.Events
            .AsNoTracking()
            .Select(e => new
            {
                e.EventId,
                e.Name,
                e.Description,
                e.EventDate,
                e.Location,
                e.DistanceKm,
                e.EventType,
                e.OrganiserId
            })
            .ToListAsync();

        return Ok(events);
    }

    /// <summary>Get details for one event.</summary>
    [HttpGet("{id:int}")]
    [AllowAnonymous]
    public async Task<IActionResult> Get(int id)
    {
        var item = await db.Events
            .AsNoTracking()
            .Where(e => e.EventId == id)
            .Select(e => new
            {
                e.EventId,
                e.Name,
                e.Description,
                e.EventDate,
                e.Location,
                e.DistanceKm,
                e.EventType,
                e.OrganiserId
            })
            .SingleOrDefaultAsync();

        return item is null ? NotFound() : Ok(item);
    }

    /// <summary>Create an event for the current organiser.</summary>
    [HttpPost]
    [Authorize(Roles = AppRoles.Organiser)]
    public async Task<IActionResult> Create(
        EventCreateRequest request)
    {
        if (!ValidEventType(request.EventType))
        {
            return BadRequest(new
            {
                message = "EventType must be run, walk, or cycle."
            });
        }

        var item = new RaceEvent
        {
            OrganiserId = CurrentUserId,
            Name = request.Name.Trim(),
            Description = request.Description.Trim(),
            EventDate = request.EventDate,
            Location = request.Location.Trim(),
            DistanceKm = request.DistanceKm,
            EventType = request.EventType
                .Trim()
                .ToLowerInvariant()
        };

        db.Events.Add(item);
        await db.SaveChangesAsync();

        return CreatedAtAction(
            nameof(Get),
            new { id = item.EventId },
            new
            {
                item.EventId,
                item.OrganiserId,
                item.Name,
                item.Description,
                item.EventDate,
                item.Location,
                item.DistanceKm,
                item.EventType
            }
        );
    }

    /// <summary>
    /// Update an event, only when the current organiser owns it.
    /// </summary>
    [HttpPut("{id:int}")]
    [Authorize(Roles = AppRoles.Organiser)]
    public async Task<IActionResult> Update(
        int id,
        EventCreateRequest request)
    {
        var item = await db.Events.FindAsync(id);

        if (item is null)
            return NotFound();

        if (item.OrganiserId != CurrentUserId)
            return Forbid();

        if (!ValidEventType(request.EventType))
        {
            return BadRequest(new
            {
                message = "EventType must be run, walk, or cycle."
            });
        }

        item.Name = request.Name.Trim();
        item.Description = request.Description.Trim();
        item.EventDate = request.EventDate;
        item.Location = request.Location.Trim();
        item.DistanceKm = request.DistanceKm;
        item.EventType = request.EventType
            .Trim()
            .ToLowerInvariant();

        await db.SaveChangesAsync();

        return Ok(new
        {
            item.EventId,
            item.Name,
            item.Description,
            item.EventDate,
            item.Location,
            item.DistanceKm,
            item.EventType
        });
    }

    /// <summary>Delete an event owned by the current organiser.</summary>
    [HttpDelete("{id:int}")]
    [Authorize(Roles = AppRoles.Organiser)]
    public async Task<IActionResult> Delete(int id)
    {
        var item = await db.Events.FindAsync(id);

        if (item is null)
            return NotFound();

        if (item.OrganiserId != CurrentUserId)
            return Forbid();

        // Database foreign-key rules protect related enrolments.
        db.Events.Remove(item);
        await db.SaveChangesAsync();

        return NoContent();
    }

    /// <summary>View categories belonging to an event.</summary>
    [HttpGet("{eventId:int}/categories")]
    [AllowAnonymous]
    public async Task<IActionResult> Categories(int eventId)
    {
        var exists = await db.Events
            .AnyAsync(e => e.EventId == eventId);

        if (!exists)
            return NotFound();

        var categories = await db.Categories
            .AsNoTracking()
            .Where(c => c.EventId == eventId)
            .Select(c => new
            {
                c.CategoryId,
                c.EventId,
                c.Name,
                c.CategoryType,
                c.MinimumAge,
                c.MaximumAge,
                c.DistanceKm
            })
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>Create a category for an event owned by the organiser.</summary>
    [HttpPost("{eventId:int}/categories")]
    [Authorize(Roles = AppRoles.Organiser)]
    public async Task<IActionResult> CreateCategory(
        int eventId,
        CategoryCreateRequest request)
    {
        var item = await db.Events.FindAsync(eventId);

        if (item is null)
            return NotFound();

        if (item.OrganiserId != CurrentUserId)
            return Forbid();

        var type = request.CategoryType
            .Trim()
            .ToLowerInvariant();

        if (type != "age" && type != "distance")
        {
            return BadRequest(new
            {
                message = "CategoryType must be age or distance."
            });
        }

        if (type == "age" &&
            (request.MinimumAge is null ||
             request.MaximumAge is null ||
             request.MinimumAge < 0 ||
             request.MaximumAge < request.MinimumAge))
        {
            return BadRequest(new
            {
                message = "Provide valid minimum and maximum ages."
            });
        }

        if (type == "distance" &&
            (request.DistanceKm is null ||
             request.DistanceKm <= 0))
        {
            return BadRequest(new
            {
                message = "Provide a positive distance."
            });
        }

        var category = new Category
        {
            EventId = eventId,
            Name = request.Name.Trim(),
            CategoryType = type,
            MinimumAge = type == "age"
                ? request.MinimumAge : null,
            MaximumAge = type == "age"
                ? request.MaximumAge : null,
            DistanceKm = type == "distance"
                ? request.DistanceKm : null
        };

        db.Categories.Add(category);
        await db.SaveChangesAsync();

        return Created(
            $"/api/events/{eventId}/categories",
            category
        );
    }
}