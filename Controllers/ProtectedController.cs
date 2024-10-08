using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Mu_MovieApis.Controllers;

[Route("Protected/{action}")]
[ApiController]
[Authorize]
public class ProtectedController : ControllerBase
{
  public IActionResult GetData() => Ok("Data from Protected Controller");
}
