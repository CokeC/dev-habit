using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.Settings;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.JsonWebTokens;
using Microsoft.IdentityModel.Tokens;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace DevHabit.Api.Services;

public sealed class TokenProvider(IOptions<JwtAuthOptions> options)
{
    private readonly JwtAuthOptions _jwtAuthOptions = options.Value;

    public AccessTokensDto Create(TokenRequest tokenRequest)
    {
        return new AccessTokensDto(GenerateAccessToken(tokenRequest), GenerateRefreshToken());
    }

    private string GenerateAccessToken(TokenRequest tokenRequest)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtAuthOptions.Key));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);
        
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, tokenRequest.UserId),
            new(ClaimTypes.Email, tokenRequest.Email)
        };
        foreach(var role in tokenRequest.Roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddMinutes(_jwtAuthOptions.ExpirationInMinutes),
            SigningCredentials = credentials,
            Issuer = _jwtAuthOptions.Issuer,
            Audience = _jwtAuthOptions.Audience
        };

        var handler = new JsonWebTokenHandler();

        var accessToken = handler.CreateToken(tokenDescriptor);

        return accessToken;
    }

    private static string GenerateRefreshToken()
    {
        var randomBytes = RandomNumberGenerator.GetBytes(32);
        
        return Convert.ToBase64String(randomBytes);
    }
}