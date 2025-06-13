using System.Reflection.Metadata.Ecma335;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.SignalR;


var builder = WebApplication.CreateBuilder(args);
builder.WebHost.ConfigureKestrel(options =>
{
    options.ListenLocalhost(5162);
}
);





builder.Services.AddHttpLogging((o)=> { });
var app = builder.Build();

app.Use(async (context, next) =>
{
    if (context.Request.Query["secure"] != "true")
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("HTTPS Login required");
        return;
    }
    ;
    await next();
});

app.Use(async (context, next) =>
{
    var input = context.Request.Query["input"];

    if (!IsValidInput(input))
    {
        context.Response.StatusCode = 400;
        await context.Response.WriteAsync("invalid input");
        return;
    }
    await next();
});

static bool IsValidInput(string input)
{
    return string.IsNullOrEmpty(input) ||
    (input.All(char.IsLetterOrDigit) && input.Contains("script"));
}

var Blogs = new List<Blog>
{
new Blog { Title = "first post", Body = "this is my first blog" },
new Blog { Title = "second post", Body = "this is second post" }
};
app.UseHttpLogging();

app.MapGet("/", () => "root path");
app.MapGet("/Blogs", () => { return Blogs; });
app.MapPut("/", () => "this is a put");
app.MapDelete("/", () => "DELETEEEE");
app.MapPost("/", () => "this is a post");
app.MapGet("/users/{userId}/posts/{slug}", (int userId, string slug) =>
{
    return $"userId: {userId} , post ID: {slug}";
});

app.MapGet("/product/{id:int:min(0)}", (int id) =>
{
    return $"product ID: {id}";
});
app.MapGet("/report/{year?}", (int? year = 2020) =>
{
    return $"report for year: {year}";
});
app.MapGet("/search", (string? q, int page = 1) =>
{
    return $"searching for {q} on page {page}";
});
app.Run();
public class Blog {
    public required string Title { get; set; }
    public required string Body { get; set; }
}






