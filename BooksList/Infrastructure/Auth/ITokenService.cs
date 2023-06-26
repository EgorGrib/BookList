using BooksList.DTOs;

namespace BooksList.Infrastructure.Auth;

public interface ITokenService
{
    string BuildToken(string key, string issuer, UserDto user);
}