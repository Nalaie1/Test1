using AutoMapper;
using Microsoft.Extensions.Caching.Memory;
using WebApplication1.Application.DTOs;
using WebApplication1.Application.Interfaces;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Services;

public class PostService : IPostService
{
    private readonly IMemoryCache _cache;
    private readonly IMapper _mapper;
    private readonly IPostRepository _repository;

    public PostService(IPostRepository repository, IMapper mapper, IMemoryCache cache)
    {
        _repository = repository;
        _mapper = mapper;
        _cache = cache;
    }

    /// <summary>
    ///     Lấy bài viết theo Id (có cache)
    /// </summary>
    public async Task<PostDto?> GetByIdAsync(Guid id)
    {
        var cacheKey = $"post_{id}";

        if (_cache.TryGetValue(cacheKey, out PostDto cached))
            return cached;

        var post = await _repository.GetByIdAsync(id);
        if (post == null)
            return null;

        var dto = _mapper.Map<PostDto>(post);

        _cache.Set(cacheKey, dto, TimeSpan.FromMinutes(5));

        return dto;
    }

    /// <summary>
    ///     Lấy danh sách bài viết có phân trang, lọc, sắp xếp
    /// </summary>
    public async Task<PagedResultDto<PostDto>> GetPagedAsync(PostQueryParametersDto parameters)
    {
        var pagedPosts = await _repository.GetPagedAsync(parameters);

        var dtoItems = _mapper.Map<List<PostDto>>(pagedPosts.Items);

        return new PagedResultDto<PostDto>
        {
            Items = dtoItems,
            TotalCount = pagedPosts.TotalCount,
            PageNumber = pagedPosts.PageNumber,
            PageSize = pagedPosts.PageSize
        };
    }

    /// <summary>
    ///     Tạo mới bài viết
    /// </summary>
    public async Task<PostDto> CreatePostAsync(PostCreateDto dto)
    {
        var entity = _mapper.Map<Post>(dto);
        var created = await _repository.CreateAsync(entity);

        return _mapper.Map<PostDto>(created);
    }

    /// <summary>
    ///     Cập nhật bài viết
    /// </summary>
    public async Task<PostDto?> UpdatePostAsync(Guid id, PostUpdateDto dto)
    {
        var updatedEntity = await _repository.UpdateAsync(id, dto.Content);
        if (updatedEntity == null)
            return null;

        // Xóa cache
        _cache.Remove($"post_{id}");

        return _mapper.Map<PostDto>(updatedEntity);
    }

    /// <summary>
    ///     Xóa bài viết
    /// </summary>
    public async Task<bool> DeletePostAsync(Guid id)
    {
        var result = await _repository.DeleteAsync(id);

        if (result)
            _cache.Remove($"post_{id}");

        return result;
    }
}