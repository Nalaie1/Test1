namespace WebApplication1.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<Post> Posts { get; set; } = new List<Post>();
    public ICollection<Comment> Comments { get; set; } = new List<Comment>();
}