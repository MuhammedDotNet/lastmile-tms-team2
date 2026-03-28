namespace LastMile.TMS.Api.GraphQL.Routes;

public sealed class CreateRouteInput
{
    public Guid VehicleId { get; set; }
    public Guid DriverId { get; set; }
    public DateTimeOffset StartDate { get; set; }
    public int StartMileage { get; set; }
    public List<Guid> ParcelIds { get; set; } = [];
}
