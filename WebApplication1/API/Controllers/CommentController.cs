// Presentation/Controllers/CommentsController.cs

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Application.DTOs;
using WebApplication1.Application.Services;

namespace WebApplication1.Presentation.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CommentsController : ControllerBase
{
    private readonly ICommentService _service;

    public CommentsController(ICommentService service)
    {
        _service = service;
    }

    // GET: api/comments/{postId}/tree
    [HttpGet("{postId}/tree")]
    [Authorize]
    public async Task<IActionResult> GetTree(Guid postId)
    {
        var comments = await _service.GetCommentTreeAsync(postId);
        return Ok(comments);
    }

    // GET: api/comments/flatten?postId={postId}
    [HttpGet("flatten")]
    [Authorize]
    public async Task<IActionResult> GetFlatten([FromQuery] Guid postId)
    {
        var comments = await _service.GetCommentFlattenAsync(postId);
        return Ok(comments);
    }

    // POST: api/comments
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] CommentCreateDto dto)
    {
        var created = await _service.CreateCommentAsync(dto);
        return CreatedAtAction(nameof(GetTree), new { postId = dto.PostId }, created);
    }

    // PUT: api/comments/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] CommentUpdateDto dto)
    {
        var updated = await _service.UpdateCommentAsync(id, dto);
        if (updated == null) return NotFound();
        return Ok(updated);
    }

    // DELETE: api/comments/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var success = await _service.DeleteCommentAsync(id);
        if (!success) return NotFound();
        return NoContent();
    }
}