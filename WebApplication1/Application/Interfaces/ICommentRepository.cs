using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Interfaces;

public interface ICommentRepository
{
    Task<List<Comment>> GetAllCommentsForPost(Guid postId, bool includeReplies = true);
    Task<List<Comment>> GetAllCommentsRecursive(Guid postId);
    Task<List<Comment>> GetAllCommentsIterative(Guid postId);
    Task<Comment> CreateAsync(Comment comment);
    Task<Comment?> UpdateAsync(Guid commentId, string newContent);
    Task<bool> DeleteAsync(Guid commentId);
}