using System.ComponentModel.DataAnnotations;

namespace Mu_MovieApis.Models.DTO;

public class LoginModel
{
  [Required]
  public string? UserName { get; set; }
  [Required]
  public string? Password { get; set; }
}
