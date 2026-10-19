namespace RaceDay.Data.Models;

public static class AppRoles
{
    public const string Organiser = "Organiser";
    public const string Participant = "Participant";
}

public sealed class User
{
    public int UserId { get; set; }

    public string FullName { get; set; } = "";

    public string Email { get; set; } = "";

    public string PasswordHash { get; set; } = "";

    public string Role { get; set; } = AppRoles.Participant;

    public DateTime CreatedUtc { get; set; } = DateTime.UtcNow;

    public ICollection<RaceEvent> Events { get; set; }
        = new List<RaceEvent>();
}