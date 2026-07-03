using DevHabit.Api.Collections;
using DevHabit.Api.DTOs.Auth;
using DevHabit.Api.Services;
using DevHabit.Api.Settings;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Text;

namespace DevHabit.UnitTests.Services;

public class TokenProviderTests
{
    private readonly TokenProvider _tokenProvider;
    private readonly JwtAuthOptions _jwtAuthOptions;

    public TokenProviderTests()
    {
        _jwtAuthOptions = new JwtAuthOptions
        {
            Issuer = "issuer",
            Audience = "audience",
            Key = "38610537-7E70-497F-B472-7B1B26E453A4",
            ExpirationInMinutes = 60,
            RefreshTokenExpirationDays = 1
        };

        var options = Options.Create(_jwtAuthOptions);
        _tokenProvider = new TokenProvider(options);
    }

    [Fact]
    public void Create_ShouldReturnBothTokens()
    {
        var tokenRequest = new TokenRequest("user", "test@example.com", [Roles.Member]);

        var result = _tokenProvider.Create(tokenRequest);

        Assert.NotNull(result.AccessToken);
        Assert.NotNull(result.RefreshToken);
    }
}
