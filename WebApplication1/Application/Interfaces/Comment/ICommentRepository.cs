using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Interfaces;

/// <summary>
///     Định nghĩa các phương thức thao tác dữ liệu bình luận
/// </summary>
public interface ICommentRepository
{
    Task<Comment?> GetByIdAsync(Guid commentId);
    Task<List<Comment>> GetAllCommentsForPost(Guid postId, bool includeReplies = true);
    Task<List<Comment>> GetAllCommentsRecursive(Guid postId);
    Task<Comment> CreateAsync(Comment comment);
    Task<Comment?> UpdateAsync(Guid commentId, string newContent);
    Task<bool> DeleteAsync(Guid commentId);
}