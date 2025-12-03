using WebApplication1.Application.DTOs;

namespace WebApplication1.Application.Interfaces.Jwt;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(string username, string password);
    Task<LoginResponseDto?> RefreshAsync(string refreshToken);
    Task LogoutAsync(Guid userId);
}