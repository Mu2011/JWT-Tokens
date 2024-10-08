using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Mu_MovieApis.Models.Domain;

public class DatabaseContext(DbContextOptions<DatabaseContext> options) :
  IdentityDbContext<ApplicationUser>(options)
{
  public DbSet<TokenInfo> TokenInfo { get; set; }
}