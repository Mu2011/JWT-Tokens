using System.Security.Claims;
using Mu_MovieApis.Models.DTO;

namespace Mu_MovieApis.Repositories.Abstract;

public interface ITokenService
{
  TokenResponse GetToken(IEnumerable<Claim> claims);
  string GetRefreshToken();
  ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
}