using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApplication1.Application.DTOs.Upload;
using WebApplication1.Application.Interfaces.IUploadService;

namespace WebApplication1.Presentation.Controllers;

[ApiController]
[Route("api/uploads")]
[Authorize(Roles = "Admin")]
public class UploadController : ControllerBase
{
    // POST: api/uploads/avatar
    [HttpPost("avatar")]
    [RequestSizeLimit(2 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadAvatar(
        [FromForm] UploadAvatarDto dto,
        [FromServices] IWebHostEnvironment env,
        [FromServices] IUploadService svc)
    {
        var idStr = User.FindFirstValue(ClaimTypes.NameIdentifier)
                    ?? User.FindFirstValue(ClaimTypes.Name)
                    ?? User.FindFirstValue("sub")
                    ?? User.FindFirstValue("id");
        if (!Guid.TryParse(idStr, out var userId))
            return Unauthorized();

        var url = await svc.UploadUserAvatarAsync(userId, dto.File, env);
        return url == null ? BadRequest("File không hợp lệ") : Ok(new { url, userId });
    }

    // POST: api/uploads/posts/{id}/attachments
    [HttpPost("posts/{id}/attachments")]
    [RequestSizeLimit(20 * 1024 * 1024)]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UploadPostAttachment(
        Guid id,
        [FromForm] UploadAttachmentDto dto,
        [FromServices] IWebHostEnvironment env,
        [FromServices] IUploadService svc)
    {
        var url = await svc.UploadPostAttachmentAsync(id, dto.File, env);
        return url == null ? BadRequest("File không hợp lệ") : Ok(new { url, postId = id });
    }
}