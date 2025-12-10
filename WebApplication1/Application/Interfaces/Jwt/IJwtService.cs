using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Interfaces.Jwt;

public interface IJwtService
{
    // Tạo Access Token chứa Claims (UserId, Username, Role)
    string GenerateAccessToken(User user);


    // Tạo Refresh Token ngẫu nhiên, độ dài an toàn 64 bytes
    string GenerateRefreshToken();

    // Validate Access Token đã hết hạn và trả về ClaimsPrincipal
    // (dùng để lấy UserId khi refresh token)
    // ClaimsPrincipal? ValidateExpiredToken(string token);
}