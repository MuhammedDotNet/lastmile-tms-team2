using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Users.DTOs;

public sealed record UserManagementUserDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Role { get; init; }
    public bool IsActive { get; init; }
    public bool IsProtected { get; init; }
    public Guid? DepotId { get; init; }
    public string? DepotName { get; init; }
    public Guid? ZoneId { get; init; }
    public string? ZoneName { get; init; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? LastModifiedAt { get; init; }

    public UserManagementUserDto() { }
}

public sealed record UserManagementUsersResultDto(
    int TotalCount,
    IReadOnlyList<UserManagementUserDto> Items);

public sealed record UserManagementRoleOptionDto(
    PredefinedRole Value,
    string Label);

public sealed record UserManagementDepotOptionDto(
    Guid Id,
    string Name);

public sealed record UserManagementZoneOptionDto(
    Guid Id,
    Guid DepotId,
    string Name);

public sealed record UserManagementLookupsDto(
    IReadOnlyList<UserManagementRoleOptionDto> Roles,
    IReadOnlyList<UserManagementDepotOptionDto> Depots,
    IReadOnlyList<UserManagementZoneOptionDto> Zones);

public sealed record UserActionResultDto(
    bool Success,
    string Message);
