using BooksList.Domain;
using BooksList.DTOs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

var users = new List<User>();

app.MapGet("/users", () => users);

app.MapGet("/users/{id:int}", (int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == id);
    return user == null ? Results.NotFound() : Results.Ok(user);
});

app.MapPost("/users", (UserDto user) =>
{
    if (string.IsNullOrEmpty(user.Name))
    {
        return Results.BadRequest("Name is required");
    }

    users.Add(new User(user.Id, user.Name));
    
    return Results.Created($"/users/{user.Id}", user);
});

app.MapPut("/users", (UserDto user) =>
{
    var index = users.FindIndex(u => u.Id == user.Id);
    if (index < 0)
    {
        return Results.NotFound("User not found");
    }
    if (string.IsNullOrEmpty(user.Name))
    {
        return Results.BadRequest("Name is required");
    }

    users[index] = new User(user.Id, user.Name);

    return Results.Ok(user);
});

app.MapDelete("/users/{id:int}", (int id) =>
{
    var index = users.FindIndex(u => u.Id == id);
    if (index < 0)
    {
        return Results.NotFound("User not found");
    }
    
    users.RemoveAt(index);
    return Results.NoContent();
});

app.MapGet("/users/{userId:int}/books", (int userId) =>
{
    var user = users.FirstOrDefault(u => u.Id == userId);
    return user == null ? Results.NotFound() : Results.Ok(user.Books);
});

app.MapGet("/users/{userId:int}/books/{id:int}", (int userId, int id) =>
{
    var user = users.FirstOrDefault(u => u.Id == userId);
    if (user == null)
    {
        return Results.NotFound("User not found");
    }
    var book = user.Books.FirstOrDefault(b => b.Id == id && b.UserId == userId);
    return book == null ? Results.NotFound() : Results.Ok(book);
});

app.MapPost("/users/{userId:int}/books", (int userId, BookDto book) =>
{
    var user = users.Find(u => u.Id == userId);
    if (user == null)
    {
        return Results.NotFound("User not found");
    }

    if (string.IsNullOrEmpty(book.Title))
    {
        return Results.BadRequest("Title is required");
    }
    
    if (string.IsNullOrEmpty(book.Author))
    {
        return Results.BadRequest("Author is required");
    }

    users.FirstOrDefault(x => x.Id == userId)!.Books
        .Add(new Book(book.Id, userId, book.Title, book.Author, book.Year, book.Genre));
    
    return Results.Created($"/users/{userId}/books/{book.Id}", book);
});

app.MapPut("/users/{userId:int}/books/{id:int}", (int userId, int id, BookDto book) =>
{
    if (id != book.Id)
    {
        return Results.BadRequest("Book ID mismatch");
    }

    var user = users.FirstOrDefault(x => x.Id == userId);
    if (user == null)
    {
        return Results.NotFound("User not found");
    }
    
    var existingBook = user.Books.FirstOrDefault(b => b.Id == id && b.UserId == userId);
    if (existingBook == null)
    {
        return Results.NotFound();
    }

    if (string.IsNullOrEmpty(book.Title))
    {
        return Results.BadRequest("Title is required");
    }
    
    if (string.IsNullOrEmpty(book.Author))
    {
        return Results.BadRequest("Author is required");
    }

    var index = user.Books.FindIndex(b => b.Id == id);
    users.FirstOrDefault(x => x.Id == userId)!
        .Books[index] = new Book(book.Id, userId, book.Title, book.Author, book.Year, book.Genre);

    return Results.Ok(book);
});

app.MapDelete("/users/{userId:int}/books/{id:int}", (int userId, int id) =>
{
    var books = users.SelectMany(x => x.Books).ToList();
    var index = books.FindIndex(b => b.Id == id && b.UserId == userId);
    
    if (index < 0)
    {
        return Results.NotFound();
    }

    users.FirstOrDefault(x => x.Id == userId)!.Books.RemoveAt(index);

    return Results.NoContent();
});



app.UseSwagger();
app.UseSwaggerUI();
app.Run();