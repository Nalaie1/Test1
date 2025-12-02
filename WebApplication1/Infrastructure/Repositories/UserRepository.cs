using Microsoft.EntityFrameworkCore;
using WebApplication1.Application.Interfaces;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Data;

namespace WebApplication1.Infrastructure.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy người dùng theo tên đăng nhập
    /// </summary>
    public async Task<User?> GetByUsernameAsync(string username)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Username == username);
    }

    /// <summary>
    /// Lấy người dùng theo ID
    /// </summary>
    public async Task<User?> GetByIdAsync(Guid id)
    {
        return await _context.Users
            .FirstOrDefaultAsync(u => u.Id == id);
    }

    /// <summary>
    /// Lưu trữ refresh token cho người dùng
    /// </summary>
    public async Task SaveRefreshTokenAsync(User user, string refreshToken, DateTime expiry)
    {
        user.RefreshToken = refreshToken;
        user.RefreshTokenExpiryTime = expiry;

        _context.Users.Update(user);
        await _context.SaveChangesAsync();
    }
}