using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Interfaces;

/// <summary>
/// Định nghĩa các phương thức thao tác dữ liệu người dùng
/// </summary>
public interface IUserRepository
{
    // Lấy user theo username, dùng cho login
    Task<User?> GetByUsernameAsync(string username);

    // Lấy user theo Id, dùng cho middleware xác thực token
    Task<User?> GetByIdAsync(Guid id);

    // Lưu refresh token
    Task SaveRefreshTokenAsync(User user, string refreshToken, DateTime expiryTime);
}