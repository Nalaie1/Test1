namespace WebApplication1.Application.DTOs
{
    public class PostCreateDto
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public Guid UserId { get; set; }  // Ai tạo bài viết
    }
}