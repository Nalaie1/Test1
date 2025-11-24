// Infrastructure/Repositories/PostRepository.cs
using System.Linq.Dynamic.Core;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Application.DTOs;
using WebApplication1.Application.Interfaces;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Data;

namespace WebApplication1.Infrastructure.Repositories;

public class PostRepository : IPostRepository
{
    private readonly AppDbContext _context;

    public PostRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<PagedResultDto<Post>> GetPagedAsync(QueryParametersDto parameters)
    {
        var query = _context.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
            .AsQueryable();

        // Filtering
        if (!string.IsNullOrEmpty(parameters.SearchTerm))
        {
            query = query.Where(p => 
                p.Title.Contains(parameters.SearchTerm) || 
                p.Content.Contains(parameters.SearchTerm));
        }

        if (parameters.UserId.HasValue)
        {
            query = query.Where(p => p.UserId == parameters.UserId.Value);
        }

        // Sorting với Dynamic LINQ
        if (!string.IsNullOrEmpty(parameters.SortBy))
        {
            var sortDirection = parameters.SortDirection?.ToLower() == "desc" ? "descending" : "ascending";
            query = query.OrderBy($"{parameters.SortBy} {sortDirection}");
        }
        else
        {
            query = query.OrderByDescending(p => p.Id); // Default sort
        }

        // Get total count BEFORE pagination
        var totalCount = await query.CountAsync();

        // Pagination
        var items = await query
            .Skip((parameters.PageNumber - 1) * parameters.PageSize)
            .Take(parameters.PageSize)
            .ToListAsync();

        return new PagedResultDto<Post>
        {
            Items = items,
            TotalCount = totalCount,
            PageNumber = parameters.PageNumber,
            PageSize = parameters.PageSize
        };
    }

    // Other methods...
    public async Task<Post?> GetByIdAsync(Guid id)
    {
        return await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<Post> CreateAsync(Post post)
    {
        post.Id = Guid.NewGuid();
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<Post?> UpdateAsync(Guid id, Post updated)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null) return null;

        post.Title = updated.Title;
        post.Content = updated.Content;
        await _context.SaveChangesAsync();
        return post;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null) return false;

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
        return true;
    }
}