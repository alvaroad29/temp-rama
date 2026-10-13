// Services/TokenService.cs
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using backend.Models;
using Microsoft.IdentityModel.Tokens;

public class TokenService : ITokenService
{
    private readonly string _secretKey;
    private readonly SymmetricSecurityKey _key;

    public TokenService(IConfiguration configuration)
    {
        _secretKey = configuration.GetValue<string>("ApiSettings:SecretKey") 
                   ?? throw new ArgumentNullException("ApiSettings:SecretKey no está configurada");
        _key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_secretKey));
    }

    public string GenerateToken(User user, IList<string> roles)
    {
        var tokenHandler = new JwtSecurityTokenHandler();
        
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id),
            new Claim(ClaimTypes.Name, user.UserName ?? string.Empty),
            new Claim(ClaimTypes.Email, user.Email ?? string.Empty)
        };

        // Agregar todos los roles
        foreach (var role in roles)
        {
            claims.Add(new Claim(ClaimTypes.Role, role));
        }

        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2),
            SigningCredentials = new SigningCredentials(_key, SecurityAlgorithms.HmacSha256Signature)
        };

        var token = tokenHandler.CreateToken(tokenDescriptor);
        return tokenHandler.WriteToken(token);
    }

    // public ClaimsPrincipal? ValidateToken(string token)
    // {
    //     var tokenHandler = new JwtSecurityTokenHandler();
        
    //     try
    //     {
    //         var principal = tokenHandler.ValidateToken(token, new TokenValidationParameters
    //         {
    //             ValidateIssuerSigningKey = true,
    //             IssuerSigningKey = _key,
    //             ValidateIssuer = false,
    //             ValidateAudience = false,
    //             ClockSkew = TimeSpan.Zero
    //         }, out _);

    //         return principal;
    //     }
    //     catch
    //     {
    //         return null;
    //     }
    // }
}