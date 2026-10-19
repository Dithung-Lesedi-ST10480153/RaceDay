using System.ComponentModel.DataAnnotations;

namespace RaceDay.API.DTOs;

public sealed record RegisterRequest(
    [Required, StringLength(120, MinimumLength = 2)]
    string FullName,

    [Required, EmailAddress, StringLength(256)]
    string Email,

    [Required, MinLength(10), StringLength(100)]
    string Password,

    [Required]
    string Role
);

public sealed record LoginRequest(
    [Required, EmailAddress]
    string Email,

    [Required]
    string Password
);

public sealed record ProfileUpdateRequest(
    [Required, StringLength(120, MinimumLength = 2)]
    string FullName
);

public sealed record UserResponse(
    int UserId,
    string FullName,
    string Email,
    string Role
);