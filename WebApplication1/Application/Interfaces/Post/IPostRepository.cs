using WebApplication1.Application.DTOs;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Interfaces;

/// <summary>
///     Định nghĩa các phương thức thao tác dữ liệu bài viết
/// </summary>
public interface IPostRepository
{
    Task<PagedResultDto<Post>> GetPagedAsync(PostQueryParametersDto parameters);
    Task<Post?> GetByIdAsync(Guid PostId);
    Task<Post> CreateAsync(Post post);
    Task<Post?> UpdateAsync(Guid PostId, string NewContent);
    Task<bool> DeleteAsync(Guid PostId);
}