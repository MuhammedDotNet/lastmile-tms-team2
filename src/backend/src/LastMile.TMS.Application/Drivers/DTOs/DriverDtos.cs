using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Application.Drivers.DTOs;

public sealed record DriverDto
{
    public Guid Id { get; init; }
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string DisplayName { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string LicenseNumber { get; init; } = string.Empty;
    public DateTimeOffset? LicenseExpiryDate { get; init; }
    public string? PhotoUrl { get; init; }
    public Guid ZoneId { get; init; }
    public string? ZoneName { get; init; }
    public Guid DepotId { get; init; }
    public string? DepotName { get; init; }
    public DriverStatus Status { get; init; }
    public Guid UserId { get; init; }
    public string? UserName { get; init; }
    public List<DriverAvailabilityDto> AvailabilitySchedule { get; init; } = [];
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; init; }
}

public sealed record DriverAvailabilityDto
{
    public Guid Id { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public TimeOnly? ShiftStart { get; init; }
    public TimeOnly? ShiftEnd { get; init; }
    public bool IsAvailable { get; init; }
}

public sealed record CreateDriverDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string LicenseNumber { get; init; } = string.Empty;
    public DateTimeOffset? LicenseExpiryDate { get; init; }
    public string? PhotoUrl { get; init; }
    public Guid ZoneId { get; init; }
    public Guid DepotId { get; init; }
    public DriverStatus Status { get; init; } = DriverStatus.Active;
    public Guid UserId { get; init; }
    public List<CreateDriverAvailabilityDto> AvailabilitySchedule { get; init; } = [];

    public CreateDriverDto() { }
}

public sealed record UpdateDriverDto
{
    public string FirstName { get; init; } = string.Empty;
    public string LastName { get; init; } = string.Empty;
    public string? Phone { get; init; }
    public string? Email { get; init; }
    public string LicenseNumber { get; init; } = string.Empty;
    public DateTimeOffset? LicenseExpiryDate { get; init; }
    public string? PhotoUrl { get; init; }
    public Guid ZoneId { get; init; }
    public Guid DepotId { get; init; }
    public DriverStatus Status { get; init; }
    public Guid UserId { get; init; }
    public List<UpdateDriverAvailabilityDto> AvailabilitySchedule { get; init; } = [];

    public UpdateDriverDto() { }
}

public sealed record CreateDriverAvailabilityDto
{
    public DayOfWeek DayOfWeek { get; init; }
    public TimeOnly? ShiftStart { get; init; }
    public TimeOnly? ShiftEnd { get; init; }
    public bool IsAvailable { get; init; }
}

public sealed record UpdateDriverAvailabilityDto
{
    public Guid? Id { get; init; }
    public DayOfWeek DayOfWeek { get; init; }
    public TimeOnly? ShiftStart { get; init; }
    public TimeOnly? ShiftEnd { get; init; }
    public bool IsAvailable { get; init; }
}
