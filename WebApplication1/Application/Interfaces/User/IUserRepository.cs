using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Interfaces;

/// <summary>
/// Định nghĩa các phương thức thao tác dữ liệu người dùng
/// </summary>
public interface IUserRepository
{
    // Phương thức lấy người dùng theo username
    Task<User?> GetByUsernameAsync(string username);
    
    // Phương thức lấy người dùng theo id
    Task<User?> GetByIdAsync(Guid id);
    
    // Phương thức lấy người dùng theo refresh token
    Task<User?> GetByRefreshTokenAsync(string refreshToken);
    
    // Phương thức cập nhật refresh token và thời gian hết hạn
    Task UpdateRefreshTokenAsync(Guid userId, string refreshToken, DateTime expiry);
    
    // Phương thức thu hồi refresh token
    Task<bool> RevokeRefreshTokenAsync(Guid userId);
}