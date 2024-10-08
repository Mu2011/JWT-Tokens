using System.ComponentModel.DataAnnotations;

namespace Mu_MovieApis.Models.DTO;

public class RegistrationModel
{
  [Required]
  public string? UserName { get; set; }
  [Required]
  public string? Name { get; set; }
  [Required]
  public string? Email { get; set; }
  [Required]
  public string? Password { get; set; }
}
