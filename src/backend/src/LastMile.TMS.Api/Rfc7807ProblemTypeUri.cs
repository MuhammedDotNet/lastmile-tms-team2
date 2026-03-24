namespace LastMile.TMS.Api;

/// <summary>
/// RFC 7231 section references for Problem Details <c>type</c> (RFC 7807).
/// </summary>
public static class Rfc7807ProblemTypeUri
{
    public const string BadRequest = "https://tools.ietf.org/html/rfc7231#section-6.5.1";
    public const string InternalServerError = "https://tools.ietf.org/html/rfc7231#section-6.6.1";
}
