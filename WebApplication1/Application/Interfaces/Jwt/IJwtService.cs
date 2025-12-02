namespace WebApplication1.Application.Interfaces;

public interface IJwtService
{
    string GenerateAccessToken(Guid userId, string username, string role);
    string GenerateRefreshToken();
    Guid? ValidateAccessToken(string token);
}