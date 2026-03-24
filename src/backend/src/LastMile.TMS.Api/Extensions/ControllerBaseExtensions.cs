using Microsoft.AspNetCore.Mvc;

namespace LastMile.TMS.Api.Extensions;

public static class ControllerBaseExtensions
{
    /// <summary>400 Problem Details for domain/business rule violations (<see cref="InvalidOperationException"/>).</summary>
    public static ObjectResult BadRequestFromBusinessRule(this ControllerBase controller, InvalidOperationException ex) =>
        controller.Problem(
            detail: ex.Message,
            statusCode: StatusCodes.Status400BadRequest,
            title: "Bad Request",
            type: Rfc7807ProblemTypeUri.BadRequest);
}
