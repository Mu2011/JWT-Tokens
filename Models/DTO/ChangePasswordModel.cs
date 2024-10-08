using System.ComponentModel.DataAnnotations;

namespace Mu_MovieApis.Models.DTO;

public class ChangePasswordModel
{
  [Required]
  public string? UserName { get; set; }
  [Required]
  public string? CurrentPassword { get; set; }
  [Required]
  public string? NewPassword { get; set; }
  [Required]
  [Compare(nameof(NewPassword), ErrorMessage = "The new password and confirmation password do not match.")]
  public string? ConfirmNewPassword { get; set; }
}