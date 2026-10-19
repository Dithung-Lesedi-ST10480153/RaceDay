namespace RaceDay.Data.Models;

public sealed class Enrolment
{
    public int EnrolmentId { get; set; }

    public int ParticipantId { get; set; }

    public User? Participant { get; set; }

    public int EventId { get; set; }

    public RaceEvent? Event { get; set; }

    public int CategoryId { get; set; }

    public Category? Category { get; set; }

    public DateTime EnrolmentDateUtc { get; set; }
        = DateTime.UtcNow;

    public RaceResult? Result { get; set; }
}