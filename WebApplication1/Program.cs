using Microsoft.EntityFrameworkCore;
using WebApplication1.Application.Interfaces;
using WebApplication1.Application.Mappings;
using WebApplication1.Application.Services;
using WebApplication1.Domain.Entities;
using WebApplication1.Infrastructure.Data;
using WebApplication1.Infrastructure.Repositories;

var builder = WebApplication.CreateBuilder(args);

// ===== Add services =====
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        "Server=nalaie\\MSSQLSERVER2022;Database=ConsoleApp2Db;Trusted_Connection=True;TrustServerCertificate=True;",
        sqlOptions => sqlOptions.CommandTimeout(600)));

// Register AutoMapper with the MappingProfile
builder.Services.AddAutoMapper(typeof(MappingProfile));

// Register repositories
builder.Services.AddScoped<ICommentRepository, CommentRepository>();
builder.Services.AddScoped<IPostRepository, PostRepository>();

// Register controllers
builder.Services.AddControllers();

builder.Services.AddScoped<ICommentService, CommentService>();

builder.Services.AddMemoryCache();

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
            .AllowAnyMethod()
            .AllowAnyHeader();
    });
});

var app = builder.Build();

// ===== Seed Database =====
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var commentRepo = scope.ServiceProvider.GetRequiredService<ICommentRepository>();
    var rand = new Random();
    var batchSize = 5000;
    var totalUsers = 100_000;
    var totalPosts = 200_000;
    var totalComments = 500_000;

    context.Database.Migrate();

    // ===== Seed Users =====
    if (!context.Users.Any())
    {
        Console.WriteLine("Seeding Users...");
        for (var i = 0; i < totalUsers; i += batchSize)
        {
            var users = new List<User>();
            for (var j = 0; j < batchSize && i + j < totalUsers; j++)
                users.Add(new User { Id = Guid.NewGuid(), Name = $"User {i + j + 1}" });
            context.Users.AddRange(users);
            context.SaveChanges();
            Console.WriteLine($"Inserted Users {i + 1} - {Math.Min(i + batchSize, totalUsers)}");
        }
    }

    // ===== Seed Posts =====
    if (!context.Posts.Any())
    {
        Console.WriteLine("Seeding Posts...");
        var allUserIds = context.Users.Select(u => u.Id).ToList();
        for (var i = 0; i < totalPosts; i += batchSize)
        {
            var posts = new List<Post>();
            for (var j = 0; j < batchSize && i + j < totalPosts; j++)
                posts.Add(new Post
                {
                    Id = Guid.NewGuid(),
                    Title = $"Post {i + j + 1}",
                    Content = $"Content {i + j + 1}",
                    UserId = allUserIds[rand.Next(allUserIds.Count)]
                });
            context.Posts.AddRange(posts);
            context.SaveChanges();
            Console.WriteLine($"Inserted Posts {i + 1} - {Math.Min(i + batchSize, totalPosts)}");
        }
    }

    // ===== Seed Comments =====
    if (!context.Comments.Any(c => c.ParentCommentId == null))
    {
        Console.WriteLine("Seeding Comments...");
        var allUserIds = context.Users.Select(u => u.Id).ToList();
        var allPostIds = context.Posts.Select(p => p.Id).ToList();

        for (var i = 0; i < totalComments; i += batchSize)
        {
            var comments = new List<Comment>();
            for (var j = 0; j < batchSize && i + j < totalComments; j++)
                comments.Add(new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = $"Comment {i + j + 1}",
                    UserId = allUserIds[rand.Next(allUserIds.Count)],
                    PostId = allPostIds[rand.Next(allPostIds.Count)],
                    ParentCommentId = null
                });
            context.Comments.AddRange(comments);
            context.SaveChanges();
            Console.WriteLine($"Inserted Comments {i + 1} - {Math.Min(i + batchSize, totalComments)}");
        }
    }

    // ===== Seed Replies =====
    if (!context.Comments.Any(c => c.ParentCommentId != null))
    {
        Console.WriteLine("Seeding Replies...");
        var allUserIds = context.Users.Select(u => u.Id).ToList();
        var parentComments = context.Comments.Where(c => c.ParentCommentId == null).ToList();

        foreach (var parent in parentComments)
        {
            var replyCount = rand.Next(0, 4);
            for (var k = 0; k < replyCount; k++)
                context.Comments.Add(new Comment
                {
                    Id = Guid.NewGuid(),
                    Content = $"Reply to {parent.Id} - {k + 1}",
                    UserId = allUserIds[rand.Next(allUserIds.Count)],
                    PostId = parent.PostId,
                    ParentCommentId = parent.Id
                });
        }

        context.SaveChanges();
        Console.WriteLine("Replies seeded!");
    }

    Console.WriteLine("Seeding completed!");
}

// ===== Swagger + Middlewares =====
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// Enable CORS
app.UseCors();
app.UseHttpsRedirection();

// Map controllers
app.MapControllers();

// ===== Demo endpoint for Comments =====
app.MapGet("/demo-comments/{postId}", async (Guid postId, ICommentRepository commentRepo) =>
    {
        var comments = await commentRepo.GetAllCommentsForPost(postId);
        return Results.Ok(comments);
    })
    .WithName("GetComments")
    .WithOpenApi();

app.Run();