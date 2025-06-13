using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.SignalR;


var builder = WebApplication.CreateBuilder(args);
builder.Services.AddHttpLogging((o)=> { });
var app = builder.Build();

app.Use(async (context, next) => { Console.WriteLine("ha"); await next.Invoke(); Console.WriteLine("haha"); });

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
