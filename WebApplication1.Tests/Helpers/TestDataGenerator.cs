using WebApplication1.Domain.Entities;

namespace WebApplication1.Tests.Helpers;

public static class TestDataGenerator
{
    /// <summary>
    /// Tạo comment tree với cấu trúc phân cấp
    /// </summary>
    /// <param name="postId">ID của post</param>
    /// <param name="userId">ID của user</param>
    /// <param name="topLevelCount">Số lượng comment cấp 1</param>
    /// <param name="maxDepth">Độ sâu tối đa của tree</param>
    /// <param name="repliesPerComment">Số lượng reply mỗi comment</param>
    /// <returns>Danh sách tất cả comments đã tạo</returns>
    public static List<Comment> GenerateCommentTree(
        Guid postId,
        Guid userId,
        int topLevelCount = 10,
        int maxDepth = 5,
        int repliesPerComment = 3)
    {
        var comments = new List<Comment>();
        var random = new Random(42); // Fixed seed for reproducibility

        // Tạo top-level comments
        for (int i = 0; i < topLevelCount; i++)
        {
            var topComment = new Comment
            {
                Id = Guid.NewGuid(),
                PostId = postId,
                UserId = userId,
                Content = $"Top-level comment {i + 1}",
                ParentCommentId = null
            };
            comments.Add(topComment);
        }

        // Tạo nested comments (replies)
        var queue = new Queue<(Comment parent, int depth)>();
        foreach (var topComment in comments.Where(c => c.ParentCommentId == null))
        {
            queue.Enqueue((topComment, 1));
        }

        while (queue.Count > 0)
        {
            var (parent, depth) = queue.Dequeue();

            if (depth >= maxDepth)
                continue;

            // Tạo replies cho comment này
            int actualReplies = random.Next(1, repliesPerComment + 1);
            for (int i = 0; i < actualReplies; i++)
            {
                var reply = new Comment
                {
                    Id = Guid.NewGuid(),
                    PostId = postId,
                    UserId = userId,
                    Content = $"Reply at depth {depth}, reply {i + 1} to comment {parent.Content}",
                    ParentCommentId = parent.Id
                };
                comments.Add(reply);
                queue.Enqueue((reply, depth + 1));
            }
        }

        return comments;
    }

    /// <summary>
    /// Tính tổng số comments trong tree
    /// </summary>
    public static int CountCommentsRecursive(List<Comment> comments, Guid? parentId = null)
    {
        var directChildren = comments.Where(c => c.ParentCommentId == parentId).ToList();
        int count = directChildren.Count;
        
        foreach (var child in directChildren)
        {
            count += CountCommentsRecursive(comments, child.Id);
        }
        
        return count;
    }

    /// <summary>
    /// Tính tổng số comments không đệ quy
    /// </summary>
    public static int CountCommentsIterative(List<Comment> comments, Guid? parentId = null)
    {
        var queue = new Queue<Guid?>();
        queue.Enqueue(parentId);
        int count = 0;

        while (queue.Count > 0)
        {
            var currentParentId = queue.Dequeue();
            var children = comments.Where(c => c.ParentCommentId == currentParentId).ToList();
            count += children.Count;

            foreach (var child in children)
            {
                queue.Enqueue(child.Id);
            }
        }

        return count;
    }
}

