using System.Net;
using System.Text;
using System.Text.Json.Serialization;
using BooksList.Domain;
using BooksList.DTOs;
using BooksList.Infrastructure;
using BooksList.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using JsonOptions = Microsoft.AspNetCore.Http.Json.JsonOptions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<BookListDb>(options =>
{
    options.UseSqlite(builder.Configuration.GetConnectionString("Sqlite"));
});

builder.Services.AddSingleton<ITokenService, TokenService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IBookRepository, BookRepository>();

builder.Services.AddAuthorization();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]))
        };
    });

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var securityScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Bearer token"
    };

    c.AddSecurityDefinition("Bearer", securityScheme);
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
            new List<string>()
        }
    });
});

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
    db.Database.EnsureCreated();
    db.Database.Migrate();
}

app.MapPost("/login", [AllowAnonymous] async (HttpContext context,
    ITokenService tokenService, IUserRepository userRepository, [FromBody] UserDto user) => 
{
    var userFromDb = await userRepository.GetUserAsync(new User(0, user.Name, user.Password));
    
    var token = tokenService.BuildToken(builder.Configuration["Jwt:Key"],
        builder.Configuration["Jwt:Issuer"], new UserDto(userFromDb.Name, userFromDb.Password));
    
    return Results.Ok(token);
});

app.MapGet("/users", [Authorize] (IUserRepository repository) 
    => repository.GetUsersAsync());

app.MapGet("/users/{id:int}", [Authorize] async (IUserRepository repository, int id) 
    => await repository.GetUserAsync(id));

app.MapPost("/users", async (IUserRepository repository, UserDto user) =>
{
    if (string.IsNullOrEmpty(user.Name)) return Results.BadRequest("Name is required");
    if (string.IsNullOrEmpty(user.Password)) return Results.BadRequest("Password is required");
    
    await repository.InsertUserAsync(new User(0, user.Name, user.Password));
    
    await repository.SaveAsync();
    return Results.Created($"/users", user);
});

app.MapPut("/users/{id:int}", [Authorize] async (IUserRepository repository, int id, UserDto user) =>
{
    var userFromDb = await repository.GetUserAsync(id);
    
    userFromDb.Name = user.Name;

    await repository.SaveAsync();
    
    return Results.Ok(user);
});

app.MapDelete("/users/{id:int}", [Authorize] async (IUserRepository repository, int id) =>
{
    await repository.DeleteUserAsync(id);

    await repository.SaveAsync();
    
    return Results.NoContent();
});

app.MapGet("/users/{userId:int}/books", [Authorize] async (IBookRepository repository, int userId) =>
{
    var books = await repository.GetBooksAsync(userId);
    return Results.Ok(books);
});

app.MapGet("/users/{userId:int}/books/{id:int}", [Authorize] async (IBookRepository repository, int userId, int id) =>
{
    var book = await repository.GetBookAsync(userId, id);
    return Results.Ok(book);
});

app.MapPost("/users/{userId:int}/books", 
    [Authorize] async (IBookRepository repository, IUserRepository userRepository, int userId, BookDto book) =>
{
    if (string.IsNullOrEmpty(book.Title)) return Results.BadRequest("Title is required");
    if (string.IsNullOrEmpty(book.Author)) return Results.BadRequest("Author is required");

    var user = await userRepository.GetUserAsync(userId);
    
    await repository
        .InsertBookAsync(userId, new Book(0, user, book.Title, book.Author, book.Year, book.Genre, userId));

    await repository.SaveAsync();
    
    return Results.Created($"/users/{userId}/books", book);
});

app.MapPut("/users/{userId:int}/books/{id:int}/status", 
    [Authorize] async (IBookRepository repository, int userId, int id, BookStatusDto book) =>
    {
        if (!Enum.IsDefined(typeof(ReadStatus), book.Status)) 
            return Results.BadRequest($"Invalid enum value: {book.Status}");

        var bookFromDb = await repository.GetBookAsync(userId, id);

        bookFromDb.Status = book.Status;
    
        await repository.UpdateBookAsync(userId, id, bookFromDb);

        await repository.SaveAsync();
    
        return Results.Ok(bookFromDb);
    });

app.MapPut("/users/{userId:int}/books/{id:int}", [Authorize]
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
    [Authorize] async (IBookRepository repository, int userId, int id) => 
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

app.UseAuthentication();
app.UseAuthorization();

app.UseSwagger();
app.UseSwaggerUI();


app.Run();