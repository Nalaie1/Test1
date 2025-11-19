using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using WebApplication1.Application.Interfaces;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Data;
using WebApplication1.Infrastructure.Repositories;
using WebApplication1.Tests.Helpers;
using Xunit;

namespace WebApplication1.Tests;

public class CommentRepositoryTests : IDisposable
{
    private readonly AppDbContext _context;
    private readonly ICommentRepository _repository;
    private readonly Guid _testPostId;
    private readonly Guid _testUserId;

    public CommentRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new AppDbContext(options);
        _repository = new CommentRepository(_context);
        
        _testPostId = Guid.NewGuid();
        _testUserId = Guid.NewGuid();

        // Setup test data
        SetupTestData();
    }

    private void SetupTestData()
    {
        // Tạo User và Post
        var user = new User
        {
            Id = _testUserId,
            Name = "Test User"
        };

        var post = new Post
        {
            Id = _testPostId,
            Title = "Test Post",
            Content = "Test Content",
            UserId = _testUserId
        };

        _context.Users.Add(user);
        _context.Posts.Add(post);
        _context.SaveChanges();

        // Tạo comment tree với nhiều levels
        var comments = TestDataGenerator.GenerateCommentTree(
            postId: _testPostId,
            userId: _testUserId,
            topLevelCount: 10,
            maxDepth: 5,
            repliesPerComment: 3
        );

        // Set User và Post navigation properties
        foreach (var comment in comments)
        {
            comment.User = user;
            comment.Post = post;
        }

        _context.Comments.AddRange(comments);
        _context.SaveChanges();
    }

    [Fact]
    public async Task GetAllCommentsRecursive_ShouldReturnAllComments()
    {
        // Act
        var result = await _repository.GetAllCommentsRecursive(_testPostId);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().BeGreaterThan(0);
        
        // Verify tất cả comments đều được load
        var totalCommentsInDb = await _context.Comments
            .Where(c => c.PostId == _testPostId)
            .CountAsync();
        
        result.Count.Should().Be(totalCommentsInDb);
    }

    [Fact]
    public async Task GetAllCommentsIterative_ShouldReturnAllTopLevelComments()
    {
        // Act
        var result = await _repository.GetAllCommentsIterative(_testPostId);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(10); // 10 top-level comments
        
        // Verify tree structure được build đúng
        var firstComment = result.First();
        firstComment.Replies.Should().NotBeEmpty();
    }

    [Fact]
    public async Task GetAllCommentsForPost_ShouldReturnTopLevelCommentsWithReplies()
    {
        // Act
        var result = await _repository.GetAllCommentsForPost(_testPostId, includeReplies: true);

        // Assert
        result.Should().NotBeNull();
        result.Count.Should().Be(10); // 10 top-level comments
        
        // Verify chỉ load 2 levels (parent và direct replies)
        var firstComment = result.First();
        firstComment.Replies.Should().NotBeEmpty();
    }

    [Fact]
    public async Task RecursiveVsIterative_ShouldReturnSameTopLevelComments()
    {
        // Act
        var recursiveResult = await _repository.GetAllCommentsRecursive(_testPostId);
        var iterativeResult = await _repository.GetAllCommentsIterative(_testPostId);

        // Assert
        var recursiveTopLevel = recursiveResult.Where(c => c.ParentCommentId == null).ToList();
        var iterativeTopLevel = iterativeResult.Where(c => c.ParentCommentId == null).ToList();

        recursiveTopLevel.Count.Should().Be(iterativeTopLevel.Count);
        recursiveTopLevel.Select(c => c.Id).Should().BeEquivalentTo(iterativeTopLevel.Select(c => c.Id));
    }

    [Fact]
    public void TestDataGenerator_ShouldCreateCorrectTreeStructure()
    {
        // Arrange
        var postId = Guid.NewGuid();
        var userId = Guid.NewGuid();
        var comments = TestDataGenerator.GenerateCommentTree(
            postId: postId,
            userId: userId,
            topLevelCount: 5,
            maxDepth: 3,
            repliesPerComment: 2
        );

        // Act
        var recursiveCount = TestDataGenerator.CountCommentsRecursive(comments);
        var iterativeCount = TestDataGenerator.CountCommentsIterative(comments);

        // Assert
        comments.Count.Should().BeGreaterThan(5);
        recursiveCount.Should().Be(comments.Count);
        iterativeCount.Should().Be(comments.Count);
        recursiveCount.Should().Be(iterativeCount);
    }

    public void Dispose()
    {
        _context.Database.EnsureDeleted();
        _context.Dispose();
    }
}

