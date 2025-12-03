using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using WebApplication1.Application.DTOs;
using WebApplication1.Application.Interfaces;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Services;

/// <summary>
/// Xử lý logic liên quan đến bình luận
/// </summary>

public class CommentService : ICommentService
{
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;
    private readonly ICommentRepository _repository;

    public CommentService(ICommentRepository repository, IMapper mapper, IMemoryCache cache)
    {
        _repository = repository;
        _mapper = mapper;
        _cache = cache;
    }

    /// <summary>
    /// Lấy cây bình luận cho một bài viết
    /// </summary>
    public async Task<List<CommentDto>> GetCommentTreeAsync(Guid postId)
    {
        // Cache key để lưu cây bình luận
        var cacheKey = $"CommentTree_{postId.ToString()}";
        if (!_cache.TryGetValue(cacheKey, out List<CommentDto> cachedTree))
        {
            var comments = await _repository.GetAllCommentsForPost(postId);
            cachedTree = MapToCommentTree(comments);
            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(cacheKey, cachedTree, cacheEntryOptions);
        }

        return cachedTree;
    }

    /// <summary>
    /// Lấy danh sách bình luận phẳng cho một bài viết
    /// </summary>
    public async Task<List<CommentFlattenDto>> GetCommentFlattenAsync(Guid postId)
    {
        var cacheKey = $"CommentFlatten_{postId.ToString()}";

        // Cache danh sách bình luận phẳng
        if (!_cache.TryGetValue(cacheKey, out List<CommentFlattenDto> cachedFlatten))
        {
            var comments = await _repository.GetAllCommentsRecursive(postId);
            cachedFlatten = FlattenComments(comments);

            var cacheEntryOptions = new MemoryCacheEntryOptions()
                .SetSlidingExpiration(TimeSpan.FromMinutes(5));

            _cache.Set(cacheKey, cachedFlatten, cacheEntryOptions);
        }

        return cachedFlatten;
    }

    /// <summary>
    /// Tạo mới bình luận
    /// </summary>
    public async Task<CommentDto> CreateCommentAsync(CommentCreateDto dto)
    {
        // Tạo đối tượng bình luận mới
        var comment = new Comment
        {
            Content = dto.Content,
            UserId = dto.UserId,
            PostId = dto.PostId,
            ParentCommentId = dto.ParentCommentId
        };

        var created = await _repository.CreateAsync(comment);
        _cache.Remove($"CommentTree_{dto.PostId.ToString()}");
        _cache.Remove($"CommentFlatten_{dto.PostId.ToString()}");
        return MapCommentRecursive(created, 0);
    }

    /// <summary>
    /// Cập nhật bình luận
    /// </summary>
    public async Task<CommentDto?> UpdateCommentAsync(Guid id, CommentUpdateDto dto)
    {
        // Cập nhật nội dung bình luận
        var updated = await _repository.UpdateAsync(id, dto.Content);
        if (updated == null) return null;

        _cache.Remove($"CommentTree_{updated.PostId.ToString()}");
        _cache.Remove($"CommentFlatten_{updated.PostId.ToString()}");
        return MapCommentRecursive(updated, 0);
    }

    /// <summary>
    /// Xóa bình luận
    /// </summary>
    public async Task<bool> DeleteCommentAsync(Guid id)
    {
        // Xóa bình luận theo id
        var comment = await _repository.GetByIdAsync(id);
        if (comment == null) return false;

        var result = await _repository.DeleteAsync(id);
        if (result)
        {
            _cache.Remove($"CommentTree_{comment.PostId.ToString()}");
            _cache.Remove($"CommentFlatten_{comment.PostId.ToString()}");
        }

        return result;
    }

    /// <summary>
    /// Map danh sách bình luận thành cây bình luận
    /// </summary>
    private List<CommentDto> MapToCommentTree(List<Comment> comments)
    {
        
        var result = new List<CommentDto>();

        foreach (var comment in comments) result.Add(MapCommentRecursive(comment, 0));

        return result;
    }

    /// <summary>
    /// Map bình luận và các phản hồi đệ quy
    /// </summary>
    private CommentDto MapCommentRecursive(Comment comment, int depth)
    {
        var dto = _mapper.Map<CommentDto>(comment);
        dto.Depth = depth;

        if (comment.Replies.Any())
            dto.Replies = comment.Replies
                .Select(reply => MapCommentRecursive(reply, depth + 1))
                .ToList();

        return dto;
    }

    /// <summary>
    /// Chuyển danh sách bình luận thành danh sách phẳng có thông tin depth và path
    /// </summary>
    private List<CommentFlattenDto> FlattenComments(List<Comment> comments)
    {
        var result = new List<CommentFlattenDto>();
        var commentDict = comments.ToDictionary(c => c.Id, c => c);
        var topLevel = comments.Where(c => c.ParentCommentId == null).ToList();

        foreach (var comment in topLevel) FlattenRecursive(comment, commentDict, result, 0, "");

        return result;
    }

    /// <summary>
    /// Đệ quy flatten bình luận và các phản hồi
    /// </summary>
    private void FlattenRecursive(Comment comment, Dictionary<Guid, Comment> allComments,
        List<CommentFlattenDto> result, int depth, string path)
    {
        var currentPath = string.IsNullOrEmpty(path) ? "1" : $"{path}.{result.Count(c => c.Path.StartsWith(path)) + 1}";

        var dto = _mapper.Map<CommentFlattenDto>(comment);
        dto.Depth = depth;
        dto.Path = currentPath;
        result.Add(dto);

        var replies = allComments.Values
            .Where(c => c.ParentCommentId == comment.Id)
            .ToList();

        foreach (var reply in replies) FlattenRecursive(reply, allComments, result, depth + 1, currentPath);
    }
}
