using Microsoft.AspNetCore.Identity;

namespace Mu_MovieApis.Models.Domain;

public class ApplicationUser : IdentityUser
{
  public string? Name { get; set; }
}