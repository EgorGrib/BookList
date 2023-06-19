using System.Net;
using System.Text.Json.Serialization;
using BooksList.Domain;
using BooksList.DTOs;
using BooksList.Infrastructure;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BookListDb>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});

builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<BookListDb>();
    //db.Database.EnsureDeleted();
    //db.Database.EnsureCreated();
    db.Database.Migrate();
}


app.MapGet("/users",  (IUserRepository repository) 
    => repository.GetUsersAsync());

app.MapGet("/users/{id:int}", async (IUserRepository repository, int id) 
    => await repository.GetUserAsync(id));

app.MapPost("/users", async (IUserRepository repository, UserDto user) =>
{
    if (string.IsNullOrEmpty(user.Name)) return Results.BadRequest("Name is required");

    await repository.InsertUserAsync(new User(user.Id, user.Name));
    
    await repository.SaveAsync();
    return Results.Created($"/users/{user.Id}", user);
});

app.MapPut("/users", async (IUserRepository repository, UserDto user) =>
{
    var userFromDb = await repository.GetUserAsync(user.Id);
    
    userFromDb.Name = user.Name;

    await repository.SaveAsync();
    
    return Results.Ok(user);
});

app.MapDelete("/users/{id:int}", async (IUserRepository repository, int id) =>
{
    await repository.DeleteUserAsync(id);

    await repository.SaveAsync();
    
    return Results.NoContent();
});

app.MapGet("/users/{userId:int}/books", async (IBookRepository repository, int userId) =>
{
    var books = await repository.GetBooksAsync(userId);
    return Results.Ok(books);
});

app.MapGet("/users/{userId:int}/books/{id:int}", async (IBookRepository repository, int userId, int id) =>
{
    var book = await repository.GetBookAsync(userId, id);
    return Results.Ok(book);
});

app.MapPost("/users/{userId:int}/books", 
    async (IBookRepository repository, IUserRepository userRepository, int userId, BookDto book) =>
{
    if (string.IsNullOrEmpty(book.Title)) return Results.BadRequest("Title is required");
    if (string.IsNullOrEmpty(book.Author)) return Results.BadRequest("Author is required");

    var user = await userRepository.GetUserAsync(userId);
    
    await repository
        .InsertBookAsync(userId, new Book(book.Id, user, book.Title, book.Author, book.Year, book.Genre, userId));

    await repository.SaveAsync();
    
    return Results.Created($"/users/{userId}/books/{book.Id}", book);
});

app.MapPut("/users/{userId:int}/books/{id:int}", 
    async (IBookRepository repository, IUserRepository userRepository, int userId, int id, UpdateBookDto book) =>
{
    if (string.IsNullOrEmpty(book.Title)) return Results.BadRequest("Title is required");
    if (string.IsNullOrEmpty(book.Author)) return Results.BadRequest("Author is required");
    if (!Enum.IsDefined(typeof(ReadStatus), book.Status)) 
        return Results.BadRequest($"Invalid enum value: {book.Status}");

    var user = await userRepository.GetUserAsync(userId);

    var updatedBook = new Book(id, user, book.Title, book.Author, book.Year, book.Genre, userId);
    
    await repository.UpdateBookAsync(userId, id, updatedBook);

    await repository.SaveAsync();
    
    return Results.Ok(updatedBook);
});

app.MapDelete("/users/{userId:int}/books/{id:int}", 
    async (IBookRepository repository, int userId, int id) => 
{
    await repository.DeleteBookAsync(userId, id);

    await repository.SaveAsync();
    
    return Results.NoContent();
});

app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
        context.Response.ContentType = "text/plain";

        var exceptionHandlerPathFeature = context.Features.Get<IExceptionHandlerPathFeature>();
        var exception = exceptionHandlerPathFeature?.Error;

        if (exception == null) 
            await context.Response.WriteAsync("An error occurred");
        else 
            await context.Response.WriteAsync(exception.Message);
    });
});

app.UseSwagger();
app.UseSwaggerUI();
app.Run();