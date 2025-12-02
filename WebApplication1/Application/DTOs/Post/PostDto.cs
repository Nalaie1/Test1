namespace WebApplication1.Application.DTOs;

/// <summary>
/// Đại diện dữ liệu cho 1 bài đăng, kèm thông tin người tạo và số lượng bình luận
/// </summary>
public class PostDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = null!;
    public string Content { get; set; } = null!;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public int CommentCount { get; set; }
}