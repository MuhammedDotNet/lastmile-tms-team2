using LastMile.TMS.Domain.Enums;

namespace LastMile.TMS.Api.GraphQL.Drivers;

public sealed class CreateDriverInput
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTimeOffset? LicenseExpiryDate { get; set; }
    public string? PhotoUrl { get; set; }
    public Guid ZoneId { get; set; }
    public Guid DepotId { get; set; }
    public DriverStatus Status { get; set; } = DriverStatus.Active;
    public Guid UserId { get; set; }
    public List<CreateDriverAvailabilityInput> AvailabilitySchedule { get; set; } = [];
}

public sealed class UpdateDriverInput
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Email { get; set; }
    public string LicenseNumber { get; set; } = string.Empty;
    public DateTimeOffset? LicenseExpiryDate { get; set; }
    public string? PhotoUrl { get; set; }
    public Guid ZoneId { get; set; }
    public Guid DepotId { get; set; }
    public DriverStatus Status { get; set; }
    public Guid UserId { get; set; }
    public List<UpdateDriverAvailabilityInput> AvailabilitySchedule { get; set; } = [];
}

public sealed class CreateDriverAvailabilityInput
{
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly? ShiftStart { get; set; }
    public TimeOnly? ShiftEnd { get; set; }
    public bool IsAvailable { get; set; }
}

public sealed class UpdateDriverAvailabilityInput
{
    public Guid? Id { get; set; }
    public DayOfWeek DayOfWeek { get; set; }
    public TimeOnly? ShiftStart { get; set; }
    public TimeOnly? ShiftEnd { get; set; }
    public bool IsAvailable { get; set; }
}
