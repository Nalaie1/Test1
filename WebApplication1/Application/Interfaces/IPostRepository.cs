using WebApplication1.Application.DTOs;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Application.Interfaces;

public interface IPostRepository
{
    Task<PagedResultDto<Post>> GetPagedAsync(QueryParametersDto parameters);
    Task<Post?> GetByIdAsync(Guid id);
    Task<Post> CreateAsync(Post post);
    Task<Post?> UpdateAsync(Guid id, Post post);
    Task<bool> DeleteAsync(Guid id);
}