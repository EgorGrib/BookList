using BooksList.Domain;
using BooksList.DTOs;
using BooksList.Infrastructure;
using Microsoft.AspNetCore.Authorization;

namespace BooksList.Apis;

public class BookApi
{
    public void Register(WebApplication app)
    {
        app.MapGet("/users/{userId:int}/books", 
            [Authorize] async (IBookRepository repository, int userId) =>
        {
            var books = await repository.GetBooksAsync(userId);
            return Results.Ok(books);
        }).WithTags("Book");
        
        app.MapGet("/books", [Authorize] async (HttpContext httpContext) =>
        {
            return await GetMyBooks(httpContext);
        }).WithTags("Book");

        app.MapGet("/users/{userId:int}/books/{id:int}", 
            [Authorize] async (IBookRepository repository, int userId, int id) =>
        {
            var book = await repository.GetBookAsync(userId, id);
            return Results.Ok(book);
        }).WithTags("Book");

        app.MapPost("/users/{userId:int}/books", 
            [Authorize] async (IBookRepository repository, 
                IUserRepository userRepository, int userId, BookDto book) =>
        {
            if (string.IsNullOrEmpty(book.Title)) return Results.BadRequest("Title is required");
            if (string.IsNullOrEmpty(book.Author)) return Results.BadRequest("Author is required");

            var user = await userRepository.GetUserAsync(userId);
            
            await repository
                .InsertBookAsync(userId, new Book(0, user, book.Title, book.Author, book.Year, book.Genre, userId));

            await repository.SaveAsync();
            
            return Results.Created($"/users/{userId}/books", book);
        }).WithTags("Book");

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
            }).WithTags("Book");

        app.MapPut("/users/{userId:int}/books/{id:int}", [Authorize]
            async (IBookRepository repository, 
                IUserRepository userRepository, int userId, int id, UpdateBookDto book) =>
        {
            if (string.IsNullOrEmpty(book.Title)) return Results.BadRequest("Title is required");
            if (string.IsNullOrEmpty(book.Author)) return Results.BadRequest("Author is required");
            if (!Enum.IsDefined(typeof(ReadStatus), book.Status)) 
                return Results.BadRequest($"Invalid enum value: {book.Status}");

            var user = await userRepository.GetUserAsync(userId);

            var updatedBook = new Book(id, user, book.Title, book.Author, book.Year, book.Genre, book.Status, userId);
            
            await repository.UpdateBookAsync(userId, id, updatedBook);

            await repository.SaveAsync();
            
            return Results.Ok(updatedBook);
        }).WithTags("Book");

        app.MapDelete("/users/{userId:int}/books/{id:int}", 
            [Authorize] async (IBookRepository repository, int userId, int id) => 
        {
            await repository.DeleteBookAsync(userId, id);

            await repository.SaveAsync();
            
            return Results.NoContent();
        }).WithTags("Book");
        
        async Task<List<Book>> GetMyBooks(HttpContext context)
        {
            using var scope = app.Services.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<BookListDb>();
            var userRepository = new UserRepository(db);
            
            var userIdClaim = context
                .User.Claims
                .FirstOrDefault(c => 
                    c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier")?.Value;

            if (userIdClaim == null || !int.TryParse(userIdClaim, out int userId)) 
                throw new InvalidOperationException("Authorization failed");

            var userFromDb = await userRepository.GetUserAsync(userId);

            if (userFromDb == null) throw new InvalidOperationException("User not found");
        
            return userFromDb.Books;
        }
    }
}