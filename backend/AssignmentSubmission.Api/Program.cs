/*
 * File: Program.cs
 * Purpose: Entry point for the ASP.NET Core Web API. Configures Dependency Injection, JWT Bearer Authentication, CORS, Swagger with Authorization UI, and HTTP Pipeline.
 * 
 * Dependencies Used:
 * - ApplicationDbContext.cs: Database Context configuration.
 * - DbInitializer.cs: Database seeding execution on startup.
 * - IJwtTokenGenerator & JwtTokenGenerator: Token creation service.
 * - IAuthService & AuthService: Authentication business logic.
 * - IAdminService & AdminService: Admin business logic.
 * - ITeacherService & TeacherService: Teacher business logic.
 * - IStudentService & StudentService: Student business logic.
 * - Microsoft.AspNetCore.Authentication.JwtBearer: JWT Authentication Middleware.
 * - Microsoft.OpenApi.Models: Swagger security scheme setup.
 * 
 * Used By:
 * - ASP.NET Core Web Host: Application entry point.
 */

using System;
using System.Text;
using AssignmentSubmission.Infrastructure.Data;
using AssignmentSubmission.Services.Implementations;
using AssignmentSubmission.Services.Interfaces;
using AssignmentSubmission.Api.Middlewares;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

// Configure Swagger with Bearer Token Authorization UI
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo { Title = "Assignment & Submission Management API", Version = "v1" });
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
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

// Configure Database Connection (Use Postgres when USE_POSTGRES=true env var is set, otherwise persistent disk SQLite DB for local runs)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
var usePostgres = Environment.GetEnvironmentVariable("USE_POSTGRES") == "true";

builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    if (usePostgres && !string.IsNullOrEmpty(connectionString))
    {
        options.UseNpgsql(connectionString);
    }
    else
    {
        // Persistent file-backed SQLite database so created data never vanishes across restarts
        options.UseSqlite("Data Source=assignment_system.db");
    }
});

// Configure Custom Application Services
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IAdminService, AdminService>();
builder.Services.AddScoped<ITeacherService, TeacherService>();
builder.Services.AddScoped<IStudentService, StudentService>();

// Configure JWT Authentication & Authorization
var secretKey = builder.Configuration["Jwt:Secret"] ?? "SuperSecretKeyForAssignmentSubmissionManagementSystem2026!";
var issuer = builder.Configuration["Jwt:Issuer"] ?? "AssignmentSubmissionApi";
var audience = builder.Configuration["Jwt:Audience"] ?? "AssignmentSubmissionApp";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
        ValidateIssuer = true,
        ValidIssuer = issuer,
        ValidateAudience = true,
        ValidAudience = audience,
        ClockSkew = TimeSpan.Zero
    };
});

builder.Services.AddAuthorization();

// Configure Rate Limiting (DDoS & Brute-force protection)
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? httpContext.Request.Headers.Host.ToString(),
            factory: partition => new FixedWindowRateLimiterOptions
            {
                AutoReplenishment = true,
                PermitLimit = 100,
                QueueLimit = 0,
                Window = TimeSpan.FromMinutes(1)
            }));
    
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Configure Strict CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "http://localhost:5000")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

var app = builder.Build();

// 1. Global Exception Handler (Top of pipeline to catch everything)
app.UseMiddleware<GlobalExceptionMiddleware>();

// 2. Security Headers (Helmet equivalents)
app.UseMiddleware<SecurityHeadersMiddleware>();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowFrontend");

// 3. Rate Limiting Middleware
app.UseRateLimiter();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Initialize and Seed Database on Startup
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.EnsureCreated();
        DbInitializer.Initialize(context);
        Console.WriteLine("[Success] Database Initialized & Seeded successfully with Persistent Storage.");
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Error] Database initialization failed: {ex.Message}");
    }
}

app.Run();
