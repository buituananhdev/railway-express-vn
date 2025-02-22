using Auth.Application.Payloads;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Auth.Application.Utils;
public class JwtUtil
{
    private static JwtSecurityTokenHandler _tokenHandler = new JwtSecurityTokenHandler();
    private static SymmetricSecurityKey GetSymmetricSecurityKey(string secretKey)
    {
        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
    }

    private static SecurityToken GenerateToken(Guid UserId, string secretKey, double expirationDays)
    {

        var key = GetSymmetricSecurityKey(secretKey);

        var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(new Claim[]
            {
                new Claim("user_id", UserId.ToString())
            }),
            Expires = DateTime.UtcNow.AddDays(expirationDays),
            SigningCredentials = creds
        };

        return _tokenHandler.CreateToken(tokenDescriptor);
    }

    public static TokenPayload GenerateAccessToken(Guid UserId, IConfiguration configuration)
    {
        var secretKey = configuration.GetSection("JwtSettings:Secret")?.Value;
        var expirationTime = double.Parse(configuration.GetSection("JwtSettings:AccessTokenExpirationTime")?.Value ?? "120");
        var accessToken = GenerateToken(UserId, secretKey, expirationTime);
        var token = new TokenPayload();
        token.Access = _tokenHandler.WriteToken(accessToken);
        return token;
    }

    public static TokenPayload GenerateAccessAndRefreshToken(Guid UserId, IConfiguration configuration)
    {
        var secretKey = configuration.GetSection("JwtSettings:Secret")?.Value;
        var accessTokenExpirationTime = double.Parse(configuration.GetSection("JwtSettings:AccessTokenExpirationTime")?.Value);
        var refreshTokenExpirationTime = double.Parse(configuration.GetSection("JwtSettings:RefreshTokenExpirationTime")?.Value);
        var accessToken = GenerateToken(UserId, secretKey, accessTokenExpirationTime);
        var refreshToken = GenerateToken(UserId, secretKey, refreshTokenExpirationTime);
        var token = new TokenPayload
        {
            Access = _tokenHandler.WriteToken(accessToken),
            Refresh = _tokenHandler.WriteToken(refreshToken)
        };
        return token;
    }
}
