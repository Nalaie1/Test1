using System.Security.Cryptography;
using System.Text;
using WebApplication1.Application.DTOs;
using WebApplication1.Application.Interfaces;
using WebApplication1.Application.Interfaces.Jwt;

namespace WebApplication1.Application.Services;

public class AuthService : IAuthService
{
    private readonly int _accessMinutes;
    private readonly string _audience;
    private readonly IConfiguration _config;

    private readonly string _issuer;
    private readonly IJwtService _jwt;
    private readonly int _refreshDays;
    private readonly string _secret;
    private readonly IUserRepository _users;

    public AuthService(IUserRepository users, IJwtService jwt, IConfiguration config)
    {
        _users = users;
        _jwt = jwt;
        _config = config;

        _issuer = _config["JwtSettings:Issuer"]!;
        _audience = _config["JwtSettings:Audience"]!;
        _secret = _config["JwtSettings:Secret"]!;

        _accessMinutes = int.Parse(_config["JwtSettings:AccessTokenMinutes"]!);
        _refreshDays = int.Parse(_config["JwtSettings:RefreshTokenDays"]!);
    }

    // ================= LOGIN =====================
    public async Task<LoginResponseDto?> LoginAsync(string username, string password)
    {
        var user = await _users.GetByUsernameAsync(username);
        if (user == null || !VerifyPassword(password, user.PasswordHash))
            return null;

        var access = _jwt.GenerateAccessToken(user);
        var refresh = _jwt.GenerateRefreshToken();

        await _users.UpdateRefreshTokenAsync(
            user.Id,
            refresh,
            DateTime.UtcNow.AddDays(_refreshDays)
        );
        return new LoginResponseDto
        {
            AccessToken = access,
            RefreshToken = refresh,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_accessMinutes)
        };
    }

    // ================= REFRESH TOKEN =====================
    public async Task<LoginResponseDto?> RefreshAsync(string refreshToken)
    {
        // Kiểm tra refresh token có tồn tại không
        var user = await _users.GetByRefreshTokenAsync(refreshToken);

        // Refresh token invalid hoặc user không tồn tại
        if (user == null)
            return null;

        // Refresh token đã hết hạn → bắt login lại
        if (user.RefreshTokenExpiryTime == null ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null;

        // Refresh token hợp lệ => cấp token mới
        var newAccessToken = _jwt.GenerateAccessToken(user);
        var newRefreshToken = _jwt.GenerateRefreshToken();

        // Lưu refresh token mới vào DB
        await _users.UpdateRefreshTokenAsync(
            user.Id,
            newRefreshToken,
            DateTime.UtcNow.AddDays(_refreshDays)
        );

        // Trả về DTO mới
        return new LoginResponseDto
        {
            AccessToken = newAccessToken,
            RefreshToken = newRefreshToken,
            ExpiresAt = DateTime.UtcNow.AddMinutes(_accessMinutes)
        };
    }

    // ================= LOGOUT =====================
    public async Task LogoutAsync(Guid userId)
    {
        await _users.RevokeRefreshTokenAsync(userId);
    }

    private bool VerifyPassword(string password, string storedHash)
    {
        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hashString = Convert.ToHexString(hashBytes);
        return hashString.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
    }
}