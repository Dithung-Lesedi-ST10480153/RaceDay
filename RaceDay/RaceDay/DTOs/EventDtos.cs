using System.ComponentModel.DataAnnotations;

using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public sealed record EventCreateRequest(
    [Required, StringLength(150)]
    string Name,

    [Required, StringLength(2000)]
    string Description,

    DateTime EventDate,

    [Required, StringLength(250)]
    string Location,

    [Range(0.1, 1000)]
    decimal DistanceKm,

    [Required]
    string EventType
);

public sealed record CategoryCreateRequest(
    [Required, StringLength(100)]
    string Name,

    [Required]
    string CategoryType,

    int? MinimumAge,
    int? MaximumAge,
    decimal? DistanceKm
);

public sealed record EnrolmentCreateRequest(
    [Range(1, int.MaxValue)]
    int EventId,

    [Range(1, int.MaxValue)]
    int CategoryId
);

public sealed record ResultCreateRequest(
    TimeSpan FinishTime,

    [Range(1, int.MaxValue)]
    int FinishingPosition
);