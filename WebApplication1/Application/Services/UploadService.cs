using WebApplication1.Application.Interfaces.IUploadService;
using WebApplication1.Infrastructure.Repositories;

namespace WebApplication1.Application.Services;

public class UploadService : IUploadService
{
    // Validate allowed file types
    private static readonly string[] AvatarExtensions = 
        { ".jpg", ".jpeg", ".png" };
    private static readonly string[] AvatarContentTypes = 
        { "image/jpeg", "image/png" };

    private static readonly string[] AttachmentExtensions = 
        { ".jpg", ".jpeg", ".png", ".pdf", ".docx", ".xlsx" };
    
    private static readonly string[] AttachmentContentTypes =
    {
        "image/jpeg", "image/png", "application/pdf",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet"
    };

    private readonly IUploadRepository _repo;

    public UploadService(IUploadRepository repo) { _repo = repo; }

    // Upload user avatar
    public async Task<string?> UploadUserAvatarAsync(Guid userId, IFormFile file, IWebHostEnvironment env)
    {
        if (!Validate(file, AvatarExtensions, AvatarContentTypes)) 
            return null;
        var (url, ok) = await SaveFileAsync(file, env, Path.Combine("uploads", "avatars"));
        if (!ok) 
            return null;
        return await _repo.SetUserAvatarAsync(userId, url) ? url : null;
    }

    // Upload post attachment
    public async Task<string?> UploadPostAttachmentAsync(Guid postId, IFormFile file, IWebHostEnvironment env)
    {
        if (!Validate(file, AttachmentExtensions, AttachmentContentTypes)) 
            return null;
        var (url, ok) = await SaveFileAsync(file, env, Path.Combine("uploads", "posts", postId.ToString()));
        if (!ok) 
            return null;
        return await _repo.SetPostImageUrlAsync(postId, url) ? url : null;
    }

    // Upload comment attachment
    public async Task<string?> UploadCommentAttachmentAsync(Guid commentId, IFormFile file, IWebHostEnvironment env)
    {
        if (!Validate(file, AttachmentExtensions, AttachmentContentTypes)) 
            return null;
        var (url, ok) = await SaveFileAsync(file, env, Path.Combine("uploads", "comments", commentId.ToString()));
        if (!ok) 
            return null;
        return await _repo.SetCommentAttachmentUrlAsync(commentId, url) ? url : null;
    }

    // Validate file extension and content type
    private static bool Validate(IFormFile file, string[] exts, string[] contentTypes)
    {
        if (file == null || file.Length == 0) 
            return false;
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!exts.Contains(ext)) 
            return false;
        if (!contentTypes.Contains(file.ContentType)) 
            return false;

        using var ms = new MemoryStream();
        file.CopyTo(ms);
        return ValidateMagic(ms.ToArray(), ext);
    }

    // Save file to disk and return URL
    private static (string url, bool ok) SaveToDisk(byte[] bytes, string ext, IWebHostEnvironment env, string folderRelative)
    {
        var root = env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
        var dir = Path.Combine(root, folderRelative);
        Directory.CreateDirectory(dir);

        var safeName = $"{Guid.NewGuid()}{ext}";
        var path = Path.Combine(dir, safeName);

        System.IO.File.WriteAllBytes(path, bytes);
        var url = "/" + folderRelative.Replace("\\", "/") + "/" + safeName;
        return (url, true);
    }

    // Save IFormFile asynchronously
    private static async Task<(string url, bool ok)> SaveFileAsync(IFormFile file, IWebHostEnvironment env, string folderRelative)
    {
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        using var ms = new MemoryStream();
        await file.CopyToAsync(ms);
        var bytes = ms.ToArray();
        var (url, ok) = SaveToDisk(bytes, ext, env, folderRelative);
        return (url, ok);
    }

    // Validate file magic numbers
    private static bool ValidateMagic(byte[] bytes, string ext)
    {
        if (bytes.Length < 8) return false;
        if (ext is ".jpg" or ".jpeg") return bytes[0] == 0xFF && bytes[1] == 0xD8;
        if (ext == ".png") return bytes[0] == 0x89 && bytes[1] == 0x50 && bytes[2] == 0x4E && bytes[3] == 0x47 &&
                                 bytes[4] == 0x0D && bytes[5] == 0x0A && bytes[6] == 0x1A && bytes[7] == 0x0A;
        if (ext == ".pdf") return bytes[0] == 0x25 && bytes[1] == 0x50 && bytes[2] == 0x44 && bytes[3] == 0x46;
        if (ext is ".docx" or ".xlsx") return bytes[0] == 0x50 && bytes[1] == 0x4B;
        return false;
    }
}