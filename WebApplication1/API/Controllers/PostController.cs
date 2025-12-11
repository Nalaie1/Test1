using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Application.DTOs;
using WebApplication1.Application.Interfaces;

namespace WebApplication1.Presentation.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class PostsController : ControllerBase
{
    private readonly IPostService _service;

    public PostsController(IPostService service)
    {
        _service = service;
    }

    // GET: api/posts
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetAll([FromQuery] PostQueryParametersDto queryParameters)
    {
        // Lấy danh sách bài viết có phân trang, lọc, sắp xếp
        var pagedResult = await _service.GetPagedAsync(queryParameters);

        // Nếu không có kết quả
        if (pagedResult.Items == null || !pagedResult.Items.Any())
            return NoContent(); // 204

        return Ok(pagedResult); // 200 + dữ liệu PagedResultDto<PostDto>
    }

    // GET: api/posts/{id}
    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetById(Guid id)
    {
        // Lấy bài viết theo id (service sẽ dùng cache nếu có)
        var post = await _service.GetByIdAsync(id);

        if (post == null)
            return NotFound(new { Message = $"Post with id '{id}' not found." });

        return Ok(post); // 200 + dữ liệu PostDto
    }

    // POST: api/posts
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create([FromBody] PostCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = await _service.CreatePostAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    // PUT: api/posts/{id}
    [HttpPut("{id}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] PostUpdateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = await _service.UpdatePostAsync(id, dto);
        if (updated == null)
            return NotFound();

        return Ok(updated);
    }

    // DELETE: api/posts/{id}
    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id)
    {
        var deleted = await _service.DeletePostAsync(id);
        if (!deleted)
            return NotFound();

        return NoContent();
    }
}