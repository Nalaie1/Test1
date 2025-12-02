using WebApplication1.Application.DTOs;

namespace WebApplication1.Application.Interfaces;

/// <summary>
/// Giao diện cho dịch vụ xác thực
/// </summary>
public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(string username, string password);
    Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken);
}