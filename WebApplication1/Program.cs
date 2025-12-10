using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Versioning;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using WebApplication1.API.Middleware;
using WebApplication1.Application.Interfaces;
using WebApplication1.Application.Interfaces.IUploadService;
using WebApplication1.Application.Interfaces.Jwt;
using WebApplication1.Application.Mappings;
using WebApplication1.Application.Services;
using WebApplication1.Infrastructure.Data;
using WebApplication1.Infrastructure.Repositories;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .Enrich.WithEnvironmentName()
    .Enrich.WithProcessId()
    .Enrich.WithThreadId()
    .WriteTo.Console()
    .WriteTo.File("Logs/log-.txt", rollingInterval: RollingInterval.Day)
    .CreateLogger();

try
{
    Log.Information("Starting API");

    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog();


    var config = builder.Configuration;

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
    builder.Services.AddScoped<IUserRepository, UserRepository>();
    builder.Services.AddScoped<IUploadRepository, UploadRepository>();

    // Register controllers
    builder.Services.AddControllers();

    // Register services
    builder.Services.AddScoped<ICommentService, CommentService>();
    builder.Services.AddScoped<IPostService, PostService>();
    builder.Services.AddScoped<IAuthService, AuthService>();
    builder.Services.AddScoped<IJwtService, JwtService>();
    builder.Services.AddScoped<IUploadService, UploadService>();

    // Add Api Versioning
    builder.Services.AddApiVersioning(options =>
    {
        options.AssumeDefaultVersionWhenUnspecified = true;
        options.DefaultApiVersion = new ApiVersion(1, 0);
        options.ReportApiVersions = true;

        // Cho phép version theo URL: /api/v1/products
        options.ApiVersionReader = new UrlSegmentApiVersionReader();
    });

    // Add Versioned API Explorer
    builder.Services.AddVersionedApiExplorer(options =>
    {
        options.GroupNameFormat = "'v'VVV";
        options.AssumeDefaultVersionWhenUnspecified = true;
    });

    // Add Memory Cache
    builder.Services.AddMemoryCache();

    // Add JWT Authentication
    builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        })
        .AddJwtBearer(options =>
        {
            // Cấu hình xác thực JWT
            options.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ValidIssuer = config["JwtSettings:Issuer"],
                ValidAudience = config["JwtSettings:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(
                    Encoding.UTF8.GetBytes(config["JwtSettings:Secret"]))
            };
        });

    // Configure Swagger to use JWT Authentication
    builder.Services.AddSwaggerGen(c =>
    {
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Name = "Authorization",
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            In = ParameterLocation.Header,
            Description = "Nhập token theo dạng: Bearer {token}"
        });

        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme
                {
                    Reference = new OpenApiReference
                    {
                        Type = ReferenceType.SecurityScheme,
                        Id = "Bearer"
                    }
                },
                Array.Empty<string>()
            }
        });
    });

    // Add Swagger
    builder.Services.AddEndpointsApiExplorer();

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

    // Add Authorization Policies
    builder.Services.AddAuthorization(options =>
    {
        options.AddPolicy("UserOrAdmin", policy => { policy.RequireRole("User", "Admin"); });
    });

    var app = builder.Build();

    // ===== Swagger + Middlewares =====
    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseHttpsRedirection();
    app.UseMiddleware<CorrelationIdMiddleware>();

    // Enable CORS
    app.UseCors("AllowAll");

    // Middlewares for Authentication & Authorization
    app.UseAuthentication();
    app.UseAuthorization();

    // Middleware
    app.UseMiddleware<RoleMiddleware>();
    app.UseMiddleware<GlobalExceptionMiddleware>();

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
    app.UseSerilogRequestLogging();

    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "API terminated unexpectedly!");
}
finally
{
    Log.CloseAndFlush();
}