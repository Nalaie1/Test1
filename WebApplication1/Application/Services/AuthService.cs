using Microsoft.EntityFrameworkCore;
using WebApplication1.Application.DTOs;
using WebApplication1.Application.Interfaces;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Data;
using WebApplication1.Infrastructure.Repositories;

namespace WebApplication1.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public AuthService(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    /// <summary>
    /// Đăng nhập người dùng và trả về token
    /// </summary>
    public async Task<LoginResponseDto?> LoginAsync(string username, string password)
    {
        var user = await _userRepository.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return null;

        // Simple password check (replace with hash in prod)
        if (user.PasswordHash != password) return null;

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Username, user.Role);
        var refreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.SaveChangesAsync();

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            Role = user.Role
        };
    }

    /// <summary>
    /// Tải mới token sử dụng refresh token
    /// </summary>
    public async Task<LoginResponseDto?> RefreshTokenAsync(string refreshToken)
    {
        var user = await _userRepository.Sa;
        if (user == null) return null;

        var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Username, user.Role);
        var newRefreshToken = _jwtService.GenerateRefreshToken();

        user.RefreshToken = newRefreshToken;
        user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
        await _userRepository.SaveChangesAsync();

        return new LoginResponseDto
        {
            AccessToken = accessToken,
            RefreshToken = newRefreshToken,
            Role = user.Role
        };
    }
}
