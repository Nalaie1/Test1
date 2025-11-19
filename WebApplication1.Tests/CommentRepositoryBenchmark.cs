using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using WebApplication1.Application.Interfaces;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Data;
using WebApplication1.Infrastructure.Repositories;
using WebApplication1.Tests.Helpers;

namespace WebApplication1.Tests;

[SimpleJob(RuntimeMoniker.Net80)]
[MemoryDiagnoser]
[MarkdownExporter]
public class CommentRepositoryBenchmark : IDisposable
{
    private AppDbContext _context = null!;
    private ICommentRepository _repository = null!;
    private Guid _testPostId;
    private Guid _testUserId;

    [Params(10, 50, 100)] // Số lượng top-level comments
    public int TopLevelCount { get; set; }

    [Params(3, 5)] // Độ sâu tối đa
    public int MaxDepth { get; set; }

    [Params(2, 4)] // Số replies mỗi comment
    public int RepliesPerComment { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"Benchmark_{Guid.NewGuid()}")
            .Options;

        _context = new AppDbContext(options);
        _repository = new CommentRepository(_context);

        _testPostId = Guid.NewGuid();
        _testUserId = Guid.NewGuid();

        // Setup test data với các tham số từ benchmark
        SetupTestData();
    }

    private void SetupTestData()
    {
        // Tạo User và Post
        var user = new User
        {
            Id = _testUserId,
            Name = "Benchmark User"
        };

        var post = new Post
        {
            Id = _testPostId,
            Title = "Benchmark Post",
            Content = "Benchmark Content",
            UserId = _testUserId
        };

        _context.Users.Add(user);
        _context.Posts.Add(post);
        _context.SaveChanges();

        // Tạo comment tree với các tham số từ benchmark
        var comments = TestDataGenerator.GenerateCommentTree(
            postId: _testPostId,
            userId: _testUserId,
            topLevelCount: TopLevelCount,
            maxDepth: MaxDepth,
            repliesPerComment: RepliesPerComment
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

    [Benchmark(Baseline = true)]
    [BenchmarkCategory("LoadAll")]
    public async Task GetAllCommentsRecursive()
    {
        var result = await _repository.GetAllCommentsRecursive(_testPostId);
        _ = result.Count; // Ensure result is materialized
    }

    [Benchmark]
    [BenchmarkCategory("LoadAll")]
    public async Task GetAllCommentsIterative()
    {
        var result = await _repository.GetAllCommentsIterative(_testPostId);
        _ = result.Count; // Ensure result is materialized
    }

    [Benchmark]
    [BenchmarkCategory("LoadAll")]
    public async Task GetAllCommentsForPost_WithReplies()
    {
        var result = await _repository.GetAllCommentsForPost(_testPostId, includeReplies: true);
        _ = result.Count; // Ensure result is materialized
    }

    [Benchmark]
    [BenchmarkCategory("LoadAll")]
    public async Task GetAllCommentsForPost_WithoutReplies()
    {
        var result = await _repository.GetAllCommentsForPost(_testPostId, includeReplies: false);
        _ = result.Count; // Ensure result is materialized
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        _context?.Database.EnsureDeleted();
        _context?.Dispose();
    }

    public void Dispose()
    {
        Cleanup();
    }
}

