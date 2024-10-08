namespace Mu_MovieApis.Models.DTO;

public class RefreshTokenRequest
{
  public string? AccessToken { get; set; }
  public string? RefreshToken { get; set; }
}