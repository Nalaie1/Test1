namespace WebApplication1.Application.Interfaces.IUploadService;

public interface IUploadService
{
    Task<string?> UploadUserAvatarAsync(Guid userId, IFormFile file, IWebHostEnvironment env);
    Task<string?> UploadPostAttachmentAsync(Guid postId, IFormFile file, IWebHostEnvironment env);
    Task<string?> UploadCommentAttachmentAsync(Guid commentId, IFormFile file, IWebHostEnvironment env);
}