using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseNpgsql(
        builder.Configuration.GetConnectionString("DefaultConnection")
    ));
// // Защита от бесконечных циклов при сериализации навигационных свойств
// // (Post → Comments → Post/User → Comments → ...) вместо исключения 500
// builder.Services.ConfigureHttpJsonOptions(options =>
// {
//     options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
// });
var app = builder.Build();  // Создаем приложение
using (var scope = app.Services.CreateScope()) // Создаем скоуп (миграции)
{
    var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    context.Database.Migrate();
}

app.MapGet("/info", () => new
{
    Service = "MinimalAPI",
    Version = "0.0.2",
    Status = "OK"
});
// Users
app.MapGet("/users/", async (AppDbContext db) => // новый список юзеров
    await db.Users.ToListAsync());
app.MapGet("/users/{id:int}", async (AppDbContext db, int id) => // получить юзера по id
{
    var user = await db.Users.FindAsync(id);
    return user == null ? Results.NotFound() : Results.Ok(user);
});
app.MapPost("/users", async (AppDbContext db, UserDto user) => // создать юзера
{
    var entity = new User
    {
        Name = user.Name,
        Email = $"{user.Name.ToLower()}@example.com",
        CreatedAt = DateTime.UtcNow
    };
    db.Users.Add(entity);
    await db.SaveChangesAsync();
    return Results.Created($"/users/{entity.Id}", entity);
});
// Posts
app.MapGet("/posts/", async (AppDbContext db) => // список постов
    await db.Posts.OrderByDescending(p => p.CreatedAt).ToListAsync());
app.MapGet("/posts/{id:int}", async (AppDbContext db, int id) => // получить пост по id
{
    var post = await db.Posts.FindAsync(id);
    return post == null ? Results.NotFound() : Results.Ok(post);
});
app.MapPost("/posts", async (AppDbContext db, PostDto post) => // создать пост
{
    var entity = new Post
    {
        Title = post.Title,
        Content = post.Content,
        CreatedAt = DateTime.UtcNow
    };
    db.Posts.Add(entity);
    await db.SaveChangesAsync();
    return Results.Created($"/posts/{entity.Id}", entity);
});
// Comments
app.MapGet("/posts/{PostId:int}/comments", async (AppDbContext db, int PostId) =>
{
    var comments = await db.Comments
        .Where(c => c.PostId == PostId)
        .OrderByDescending(c => c.CreatedAt)
        .ToListAsync();
    return Results.Ok(comments);
});
app.MapPost("/posts/{PostId:int}/comments", async (AppDbContext db, int PostId, CommentDto commentDto) =>
{
    var post = await db.Posts.FindAsync(PostId);
    if (post == null) return Results.NotFound();
    var user = await db.Users.FindAsync(commentDto.UserId);
    if (user == null) return Results.NotFound();
    var comment = new Comment
    {
        PostId = PostId,
        UserId = commentDto.UserId,
        Content = commentDto.Text,
        CreatedAt = DateTime.UtcNow
    };
    db.Comments.Add(comment);
    await db.SaveChangesAsync();
    return Results.Created($"/posts/{PostId}/comments/{comment.Id}", comment);
});

app.Run();

// Объявления типов (record, class) идут ПОСЛЕ всех инструкций верхнего уровня
record UserDto(string Name, int Age);
record PostDto(string Title, string Content);
record CommentDto(string Text, int UserId);