using Microsoft.AspNetCore.Mvc;
using WebApplication1.Application.Interfaces;
using WebApplication1.Domain.Entities;

namespace WebApplication1.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentRepository _repo;

    public CommentsController(ICommentRepository repo)
    {
        _repo = repo;
    }

    // GET: api/comments/{postId}/tree
    [HttpGet("{postId}/tree")]
    public async Task<IActionResult> GetTree(Guid postId)
    {
        var comments = await _repo.GetAllCommentsForPost(postId);
        return Ok(comments);
    }

    // GET: api/comments/{postId}/flat
    [HttpGet("{postId}/flat")]
    public async Task<IActionResult> GetFlat(Guid postId)
    {
        var comments = await _repo.GetAllCommentsRecursive(postId);
        return Ok(comments);
    }

    // POST: api/comments
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] Comment comment)
    {
        var created = await _repo.CreateAsync(comment);
        return CreatedAtAction(nameof(GetTree), new { postId = comment.PostId }, created);
    }

    // PUT: api/comments/{id}
    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] Comment comment)
    {
        var updated = await _repo.UpdateAsync(id, comment.Content);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    // DELETE: api/comments/{id}
    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _repo.DeleteAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}