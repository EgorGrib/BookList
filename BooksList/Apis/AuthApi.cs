using BooksList.Domain;
using BooksList.DTOs;
using BooksList.Infrastructure;
using BooksList.Infrastructure.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BooksList.Apis;

public class AuthApi
{
    public void Register(WebApplication app)
    {
        app.MapPost("/login", [AllowAnonymous] async (HttpContext context,
            ITokenService tokenService, IUserRepository userRepository, [FromBody] LoginUserDto user) => 
        {
            var userFromDb = await userRepository.GetUserAsync(user);
        
            var token = tokenService.BuildToken(app.Configuration["Jwt:Key"],
                app.Configuration["Jwt:Issuer"], new UserDto(userFromDb.Id, userFromDb.Name, userFromDb.Password));
        
            return Results.Ok(token);
        }).WithTags("Auth");
        
        app.MapPost("/register", async (IUserRepository repository, LoginUserDto user) =>
        {
            if (string.IsNullOrEmpty(user.Name)) return Results.BadRequest("Name is required");
            if (string.IsNullOrEmpty(user.Password)) return Results.BadRequest("Password is required");
            
            await repository.InsertUserAsync(new User(0, user.Name, user.Password));
            
            await repository.SaveAsync();
            return Results.Created($"/users", user);
        }).WithTags("Auth");
    }
}