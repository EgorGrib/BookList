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
    }
}