namespace LibrarySystem.API.Filters;

public class ValidationFilter : IEndpointFilter
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        // Add validation logic here before passing to the next delegate
        return await next(context);
    }
}
