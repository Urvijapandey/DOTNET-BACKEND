using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;

namespace MinimalUserApi
{
    // User model (nullable strings for compatibility)
    public class User
    {
        public int Id { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
    }

    // Middleware for authentication and logging
    public class AuthLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<AuthLoggingMiddleware> _logger;

        public AuthLoggingMiddleware(RequestDelegate next, ILogger<AuthLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            _logger.LogInformation($"Incoming request: {context.Request.Method} {context.Request.Path}");

            if (!context.Request.Headers.ContainsKey("X-Auth-Token"))
            {
                _logger.LogWarning("Missing authentication token.");
                context.Response.StatusCode = 401;
                await context.Response.WriteAsync("Unauthorized");
                return;
            }

            await _next(context);
        }
    }

    // In-memory repository for managing users
    public class UserRepo
    {
        private readonly List<User> _users = new()
        {
            new User { Id = 1, Username = "admin", Email = "admin@example.com" }
        };

        public List<User> GetAll() => _users;

        public User? Get(int id) => _users.FirstOrDefault(u => u.Id == id);

        public User Add(User user)
        {
            var newUser = new User
            {
                Id = _users.Any() ? _users.Max(u => u.Id) + 1 : 1,
                Username = user.Username,
                Email = user.Email
            };
            _users.Add(newUser);
            return newUser;
        }

        public void Update(User updated)
        {
            var existing = Get(updated.Id);
            if (existing != null)
            {
                existing.Username = updated.Username;
                existing.Email = updated.Email;
            }
        }

        public void Delete(int id)
        {
            var user = Get(id);
            if (user != null) _users.Remove(user);
        }
    }

    // Minimal API setup
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddSingleton<UserRepo>();
            builder.Services.AddLogging();
            builder.Services.AddControllers();

            var app = builder.Build();

            app.UseMiddleware<AuthLoggingMiddleware>();

            app.MapGet("/api/users", ([FromServices] UserRepo repo) =>
            {
                return Results.Ok(repo.GetAll());
            });

            app.MapGet("/api/users/{id}", ([FromServices] UserRepo repo, int id) =>
            {
                var user = repo.Get(id);
                return user == null ? Results.NotFound() : Results.Ok(user);
            });

            app.MapPost("/api/users", ([FromServices] UserRepo repo, [FromBody] User user) =>
            {
                if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                    return Results.BadRequest("Invalid data");

                var createdUser = repo.Add(user);
                return Results.Created($"/api/users/{createdUser.Id}", createdUser);
            });

            app.MapPut("/api/users/{id}", ([FromServices] UserRepo repo, int id, [FromBody] User user) =>
            {
                var existing = repo.Get(id);
                if (existing == null) return Results.NotFound();

                if (string.IsNullOrWhiteSpace(user.Username) || string.IsNullOrWhiteSpace(user.Email))
                    return Results.BadRequest("Invalid data");

                user.Id = id;
                repo.Update(user);
                return Results.NoContent();
            });

            app.MapDelete("/api/users/{id}", ([FromServices] UserRepo repo, int id) =>
            {
                var existing = repo.Get(id);
                if (existing == null) return Results.NotFound();

                repo.Delete(id);
                return Results.NoContent();
            });

            app.Run();
        }
    }
}









