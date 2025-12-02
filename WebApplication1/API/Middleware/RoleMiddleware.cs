using System.Security.Claims;

namespace WebApplication1.API.Middleware;

public class RoleMiddleware
{
    private readonly RequestDelegate _next;
    private readonly string _role;
    
    public RoleMiddleware (RequestDelegate next, string role)
    {
        _next = next;
        _role = role;
    }
    public async Task InvokeAsync(HttpContext context)
    {
        // Kiểm tra user đã login
        if (!context.User.Identity?.IsAuthenticated ?? true)
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            await context.Response.WriteAsync("Unauthorized");
            return;
        }

        // Lấy role từ claim
        var role = context.User.FindFirst(ClaimTypes.Role)?.Value;

        if (role != _role)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            await context.Response.WriteAsync("Forbidden: insufficient role");
            return;
        }

        // Role đúng → chuyển tiếp request
        await _next(context);
    }
}