using System.Security.Cryptography;
using System.Text;
using WebApplication1.Application.DTOs;
using WebApplication1.Application.Interfaces;
using WebApplication1.Application.Interfaces.Jwt;

namespace WebApplication1.Application.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IJwtService _jwt;
    private readonly IConfiguration _config;

    private readonly string _issuer;
    private readonly string _audience;
    private readonly string _secret;
    private readonly int _accessMinutes;
    private readonly int _refreshDays;

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
    private bool VerifyPassword(string password, string storedHash)
    {
        using var sha = SHA256.Create();
        var hashBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
        var hashString = Convert.ToHexString(hashBytes);
        return hashString.Equals(storedHash, StringComparison.OrdinalIgnoreCase);
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
        // 1) Kiểm tra refresh token có tồn tại không
        var user = await _users.GetByRefreshTokenAsync(refreshToken);

        // 2) Refresh token invalid hoặc user không tồn tại
        if (user == null)
            return null;

        // 3) Refresh token đã hết hạn → bắt login lại
        if (user.RefreshTokenExpiryTime == null ||
            user.RefreshTokenExpiryTime <= DateTime.UtcNow)
            return null;

        // 4) Refresh token hợp lệ => cấp token mới
        var newAccessToken = _jwt.GenerateAccessToken(user);
        var newRefreshToken = _jwt.GenerateRefreshToken();

        // 5) Lưu refresh token mới vào DB
        await _users.UpdateRefreshTokenAsync(
            user.Id,
            newRefreshToken,
            DateTime.UtcNow.AddDays(_refreshDays)
        );

        // 6) Trả về DTO mới
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
}
