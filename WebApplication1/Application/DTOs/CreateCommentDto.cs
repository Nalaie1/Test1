namespace WebApplication1.Application.DTOs;

public class CreateCommentDto
{
    public string Content { get; set; } = null!;
    public Guid UserId { get; set; }
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }
}