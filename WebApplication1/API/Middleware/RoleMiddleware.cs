namespace WebApplication1.API.Middleware;

/// <summary>
/// Middleware bảo vệ dữ liệu dựa trên vai trò người dùng
/// </summary>
public class RoleMiddleware
{
    private readonly RequestDelegate _next;

    public RoleMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    // Kiểm tra vai trò người dùng dựa trên đường dẫn yêu cầu
    public async Task Invoke(HttpContext context)
    {
        var path = context.Request.Path.Value?.ToLowerInvariant() ?? string.Empty;
        // if (path.StartsWith("/admin"))
        // {
        //     if (!context.User.Identity?.IsAuthenticated || !context.User.IsInRole("Admin"))
        //     {
        //         context.Response.StatusCode = StatusCodes.Status403Forbidden;
        //         return;
        //     }
        //     else
        //     {
        //          context.Response.StatusCode = StatusCodes.Status403Forbidden;
        //          return;
        //     }
        // }
        // else if (path.StartsWith("/user"))
        // {
        //     if (!context.User.Identity?.IsAuthenticated ?? true)
        //     {
        //         context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        //         return;
        //     }
        // }
        if (path.StartsWith("/admin"))
        {
            bool isAuth = context.User.Identity?.IsAuthenticated == true;
            bool isAdmin = context.User.IsInRole("Admin");
            if (!isAuth || !isAdmin)
            {
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                return;
            }
        }
        else if (path.StartsWith("/user"))
        {
            bool isAuth = context.User.Identity?.IsAuthenticated == true;
            if (!isAuth)
            {
                context.Response.StatusCode = StatusCodes.Status401Unauthorized;
                return;
            }
        }
        await _next(context);
    }
}