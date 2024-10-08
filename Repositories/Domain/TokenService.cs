using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.IdentityModel.Tokens;
using Mu_MovieApis.Models.DTO;
using Mu_MovieApis.Repositories.Abstract;

namespace Mu_MovieApis.Repositories.Domain;

public class TokenService(IConfiguration configuration) : ITokenService
{
  private readonly IConfiguration _configuration = configuration;
  public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
  {
    var tokenValidationParameters = new TokenValidationParameters
    {
      ValidateAudience = false,
      ValidateIssuer = false,
      ValidateIssuerSigningKey = true,
      IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"])),
      ValidateLifetime = false
    };

    var tokenHandler = new JwtSecurityTokenHandler();
    SecurityToken securityToken;

    // Principal 
    var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out securityToken);
    var jwtSecurityToken = securityToken as JwtSecurityToken;
    if (jwtSecurityToken is null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
      throw new SecurityTokenException("Invalid token");

    return principal;
  }

  public string GetRefreshToken()
  {
    var randomNumber = new byte[32];
    using var rng = RandomNumberGenerator.Create();
    rng.GetBytes(randomNumber);
    return Convert.ToBase64String(randomNumber);
  }

  public TokenResponse GetToken(IEnumerable<Claim> claims)
  {
    var secretKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT:Key"]));
    var tokenOptions = new JwtSecurityToken(
        issuer: _configuration["JWT:Issuer"],
        audience: _configuration["JWT:Audience"],
        claims: claims,
        expires: DateTime.Now.AddMinutes(5),
        signingCredentials: new SigningCredentials(secretKey, SecurityAlgorithms.HmacSha256)
    );
    var tokenString = new JwtSecurityTokenHandler().WriteToken(tokenOptions);

    return new TokenResponse
    {
      TokenString = tokenString,
      ValidTo = tokenOptions.ValidTo
    };
  }
}