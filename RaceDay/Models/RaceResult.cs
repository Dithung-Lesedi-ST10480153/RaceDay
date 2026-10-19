namespace RaceDay.Data.Models;

public sealed class RaceResult
{
    public int ResultId { get; set; }

    public int EnrolmentId { get; set; }

    public Enrolment? Enrolment { get; set; }

    public TimeSpan FinishTime { get; set; }

    public int FinishingPosition { get; set; }

    public DateTime RecordedUtc { get; set; }
        = DateTime.UtcNow;
}