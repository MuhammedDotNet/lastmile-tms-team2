using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Users.DTOs;

public sealed record CreateUserDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public PredefinedRole Role { get; init; }
    public Guid? DepotId { get; init; }
    public Guid? ZoneId { get; init; }

    public CreateUserDto() { }
}

public sealed record UpdateUserDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public PredefinedRole Role { get; init; }
    public Guid? DepotId { get; init; }
    public Guid? ZoneId { get; init; }
    public bool IsActive { get; init; }

    public UpdateUserDto() { }
}
