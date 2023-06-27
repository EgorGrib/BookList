using BooksList.Domain;
using BooksList.DTOs;
using BooksList.Infrastructure;
using BooksList.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksList.Apis;

public class UserApi
{
    public void Register(WebApplication app)
    {
        app.MapGet("/users", [Authorize](IUserRepository repository)
            => repository.GetUsersAsync())
            .WithTags("User");

        app.MapGet("/users/{id:int}", [Authorize] async (IUserRepository repository, int id) 
            => await repository.GetUserAsync(id))
            .WithTags("User");
        
        app.MapGet("/user", [Authorize] async (HttpContext context) =>
        {
            return await GetMyUser(context);
        }).WithTags("User");

        app.MapPost("/users", async (IUserRepository repository, LoginUserDto user) =>
        {
            if (string.IsNullOrEmpty(user.Name)) return Results.BadRequest("Name is required");
            if (string.IsNullOrEmpty(user.Password)) return Results.BadRequest("Password is required");
            
            await repository.InsertUserAsync(new User(0, user.Name, user.Password));
            
            await repository.SaveAsync();
            return Results.Created($"/users", user);
        }).WithTags("User");

        app.MapPut("/users/{id:int}", 
            [Authorize] async (IUserRepository repository, int id, LoginUserDto user) =>
        {
            var userFromDb = await repository.GetUserAsync(id);
            
            userFromDb.Name = user.Name;

            await repository.SaveAsync();
            
            return Results.Ok(user);
        }).WithTags("User");

        app.MapDelete("/users/{id:int}", [Authorize] async (IUserRepository repository, int id) =>
        {
            await repository.DeleteUserAsync(id);

            await repository.SaveAsync();
            
            return Results.NoContent();
        }).WithTags("User");
        
        async Task<User> GetMyUser(HttpContext context)
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
        
            return userFromDb;
        }
    }
}