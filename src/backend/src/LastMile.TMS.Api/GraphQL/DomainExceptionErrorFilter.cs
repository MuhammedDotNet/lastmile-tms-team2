namespace LastMile.TMS.Api.GraphQL;

/// <summary>
/// Hot Chocolate replaces resolver exceptions with "Unexpected Execution Error" in the GraphQL
/// <c>message</c> field by default. Map domain <see cref="InvalidOperationException"/> to the
/// public message so clients (e.g. dispatcher toasts) show the real reason.
/// </summary>
public sealed class DomainExceptionErrorFilter : IErrorFilter
{
    public IError OnError(IError error)
    {
        if (error.Exception is InvalidOperationException ex)
        {
            return error.WithMessage(ex.Message);
        }

        return error;
    }
}
