using backend.Models;

public interface ITokenService
{
    string GenerateToken(User user, IList<string> roles);
    // ClaimsPrincipal? ValidateToken(string token);
}