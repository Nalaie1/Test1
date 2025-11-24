using WebApplication1.Application.DTOs;

namespace WebApplication1.Application.Services;

public interface ICommentService
{
    Task<List<CommentDto>> GetCommentTreeAsync(Guid postId);
    Task<List<CommentFlattenDto>> GetCommentFlattenAsync(Guid postId);
    Task<CommentDto> CreateCommentAsync(CreateCommentDto dto);
    Task<CommentDto?> UpdateCommentAsync(Guid id, UpdateCommentDto dto);
    Task<bool> DeleteCommentAsync(Guid id);
}