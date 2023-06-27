using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BooksList.DTOs;
using Microsoft.IdentityModel.Tokens;

namespace BooksList.Infrastructure.Auth;

public class TokenService : ITokenService
{
    private readonly TimeSpan _expiryDuration = new TimeSpan(0, 30, 0);
    
    public string BuildToken(string key, string issuer, UserDto user)
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.Name, user.Name),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString())
        };

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key));
        var credentials = new SigningCredentials(securityKey,
            SecurityAlgorithms.HmacSha256Signature);
        var tokenDescriptor = new JwtSecurityToken(issuer, issuer, claims,
            expires: DateTime.Now.Add(_expiryDuration), signingCredentials: credentials);
        
        return new JwtSecurityTokenHandler().WriteToken(tokenDescriptor);
    }
}