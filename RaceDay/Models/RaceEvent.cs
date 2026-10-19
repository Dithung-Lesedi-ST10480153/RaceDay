namespace RaceDay.Data.Models;

public sealed class RaceEvent
{
    public int EventId { get; set; }

    public int OrganiserId { get; set; }

    public User? Organiser { get; set; }

    public string Name { get; set; } = "";

    public string Description { get; set; } = "";

    public DateTime EventDate { get; set; }

    public string Location { get; set; } = "";

    public decimal DistanceKm { get; set; }

    // Allowed values: run, walk, cycle.
    public string EventType { get; set; } = "run";

    public ICollection<Category> Categories { get; set; }
        = new List<Category>();
}