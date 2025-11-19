using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Data;

namespace WebApplication1.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController : ControllerBase
{
    private readonly AppDbContext _context;

    public PostsController(AppDbContext context)
    {
        _context = context;
    }

    // GET: api/posts
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var posts = await _context.Posts
            .Include(p => p.User)
            .ToListAsync();
        return Ok(posts);
    }

    // GET: api/posts/{id}
    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var post = await _context.Posts
            .Include(p => p.User)
            .Include(p => p.Comments)
            .FirstOrDefaultAsync(p => p.Id == id);
        if (post == null) return NotFound();
        return Ok(post);
    }

    // POST: api/posts
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Post post)
    {
        post.Id = Guid.NewGuid();
        _context.Posts.Add(post);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = post.Id }, post);
    }

    // PUT: api/posts/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Post updated)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null) return NotFound();

        post.Title = updated.Title;
        post.Content = updated.Content;

        await _context.SaveChangesAsync();
        return Ok(post);
    }

    // DELETE: api/posts/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var post = await _context.Posts.FindAsync(id);
        if (post == null) return NotFound();

        _context.Posts.Remove(post);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}