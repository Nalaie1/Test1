namespace WebApplication1.Application.DTOs;

public class CommentDto
{
    public Guid Id { get; set; }
    public string Content { get; set; } = null!;
    public Guid UserId { get; set; }
    public string UserName { get; set; } = null!;
    public Guid PostId { get; set; }
    public Guid? ParentCommentId { get; set; }
    public List<CommentDto> Replies { get; set; } = new(); // Recursive DTO
    public int Depth { get; set; } // Optional: track depth
}