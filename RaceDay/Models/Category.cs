namespace RaceDay.Data.Models;

public sealed class Category
{
    public int CategoryId { get; set; }

    public int EventId { get; set; }

    public RaceEvent? Event { get; set; }

    public string Name { get; set; } = "";

    // Allowed values: age, distance.
    public string CategoryType { get; set; } = "age";

    public int? MinimumAge { get; set; }

    public int? MaximumAge { get; set; }

    public decimal? DistanceKm { get; set; }

    public ICollection<Enrolment> Enrolments { get; set; }
        = new List<Enrolment>();
}