using Microsoft.AspNetCore.Mvc;
using Mu_MovieApis.Models.DTO;
using Mu_MovieApis.Models.Domain;
using Mu_MovieApis.Repositories.Abstract;
using Microsoft.AspNetCore.Authorization;

namespace Mu_MovieApis.Controllers;

[Route("Token/{action}")]
[ApiController]
public class TokenController(DatabaseContext databaseContext, ITokenService tokenService) : ControllerBase
{
  private readonly DatabaseContext _databaseContext = databaseContext;
  private readonly ITokenService _tokenService = tokenService;

  [HttpPost]
  public IActionResult RefreshToken([FromBody] RefreshTokenRequest tokenApiModel)
  {
    if (tokenApiModel is null)
      return BadRequest("Invalid client request");

    var accessToken = tokenApiModel.AccessToken;
    var refreshToken = tokenApiModel.RefreshToken;

    var principal = _tokenService.GetPrincipalFromExpiredToken(accessToken);
    var username = principal.Identity.Name;

    var user = _databaseContext.TokenInfo.FirstOrDefault(u => u.Username == username);

    if (user is null || user.RefreshToken != refreshToken || user.RefreshTokenExpiry <= DateTime.Now)
      return BadRequest("Invalid client request");

    var newAccessToken = _tokenService.GetToken(principal.Claims);
    var newRefreshToken = _tokenService.GetRefreshToken();
    user.RefreshToken = newRefreshToken;
    _databaseContext.TokenInfo.Update(user);
    _databaseContext.SaveChanges();

    return Ok(new RefreshTokenRequest
    {
      AccessToken = newAccessToken.TokenString,
      RefreshToken = newRefreshToken
    });
  }

  // revoking is use removing token entry
  [HttpPost, Authorize]
  public IActionResult RevokeToken()
  {
    try
    {
      var username = User.Identity.Name;
      var token = _databaseContext.TokenInfo.FirstOrDefault(u => u.Username == username);
      if (token is null) return BadRequest("Invalid client request");
      _databaseContext.TokenInfo.Remove(token);
      _databaseContext.SaveChanges();
      return Ok("Token revoked");
    }
    catch (Exception ex)
    {
      return BadRequest($"Error: {ex.Message}");
    }
  }
}