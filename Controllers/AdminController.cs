using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mu_MovieApis.Controllers;

[Route("Admin/{action}")]
[ApiController]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
  public IActionResult GetData() => Ok("Data from Admin Controller");
}