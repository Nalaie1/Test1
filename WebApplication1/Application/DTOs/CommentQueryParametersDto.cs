namespace WebApplication1.Application.DTOs;

public class CommentQueryParametersDto
{
    public Guid? PostId { get; set; }
    public Guid? UserId { get; set; }
    public string? SearchTerm { get; set; }  // Filter theo content
    public string? SortBy { get; set; }      // cột để sort
    public string? SortDirection { get; set; } // "asc" / "desc"
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public bool IncludeReplies { get; set; } = true;
}