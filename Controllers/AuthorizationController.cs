using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Mu_MovieApis.Models;
using Mu_MovieApis.Models.Domain;
using Mu_MovieApis.Models.DTO;
using Mu_MovieApis.Repositories.Abstract;

namespace Mu_MovieApis.Controllers;

[Route("Authorization/{action}")]
public class AuthorizationController(DatabaseContext databaseContext, UserManager<ApplicationUser> userManager,
RoleManager<IdentityRole> roleManager, ITokenService tokenService) : ControllerBase
{
  private readonly DatabaseContext _databaseContext = databaseContext;
  private readonly UserManager<ApplicationUser> _userManager = userManager;
  private readonly RoleManager<IdentityRole> _roleManager = roleManager;
  private readonly ITokenService _tokenService = tokenService;

  [HttpPost]
  public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordModel model)
  {
    var status = new Status();

    // Check if model is valid
    if (!ModelState.IsValid)
    {
      status.StatusCode = 0;
      status.Message = "Please pass all the valid fields";
      return Ok();
    }

    // lets find the user
    var user = await _userManager.FindByNameAsync(model.UserName);
    if (user is null)
    {
      status.StatusCode = 0;
      status.Message = "Invalid User Name";
      return Ok();
    }

    // check if current password
    var check = await _userManager.CheckPasswordAsync(user, model.CurrentPassword);
    if (!check)
    {
      status.StatusCode = 0;
      status.Message = "Invalid Current Password";
      return Ok();
    }

    // Change Password here
    var result = await _userManager.ChangePasswordAsync(user, model.CurrentPassword, model.NewPassword);
    if (!result.Succeeded)
    {
      status.StatusCode = 0;
      status.Message = "Filed to Change Password";
      return Ok();
    }

    status.StatusCode = 1;
    status.Message = "Password Changed Successfully";
    return Ok(status);
  }

  [HttpPost]
  public async Task<IActionResult> Login([FromBody] LoginModel model)
  {
    var user = await _userManager.FindByNameAsync(model.UserName);

    if (user is not null && await _userManager.CheckPasswordAsync(user, model.Password))
    {
      var userRoles = await _userManager.GetRolesAsync(user);

      var authClaims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, user.UserName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
        };

      foreach (var userRole in userRoles)
      {
        authClaims.Add(new Claim(ClaimTypes.Role, userRole));
      }

      var tokenResponse = _tokenService.GetToken(authClaims); // Direct call
      var refreshToken = _tokenService.GetRefreshToken();
      var tokenInfo = _databaseContext.TokenInfo.FirstOrDefault(u => u.Username == user.UserName);

      if (tokenInfo is null)
      {
        _databaseContext.TokenInfo.Add(new TokenInfo
        {
          Username = user.UserName,
          RefreshToken = refreshToken,
          RefreshTokenExpiry = DateTime.Now.AddDays(7)
        });
      }
      else
      {
        tokenInfo.RefreshToken = refreshToken;
        tokenInfo.RefreshTokenExpiry = DateTime.Now.AddDays(7);
      }

      try
      {
        await _databaseContext.SaveChangesAsync(); // Use async
      }
      catch (Exception ex)
      {
        return BadRequest($"Error: {ex.Message}");
      }

      return Ok(new LoginResponse
      {
        Name = user.Name,
        Username = user.UserName,
        Token = tokenResponse.TokenString,
        RefreshToken = refreshToken,
        Expiration = tokenResponse.ValidTo,
        StatusCode = 1,
        Message = "Login Successful"
      });
    }

    return Ok(new LoginResponse
    {
      StatusCode = 0,
      Message = "Invalid User Name or Password",
      Name = null,
      Token = string.Empty,
      Expiration = null
    });

  }

  [HttpPost]
  public async Task<IActionResult> Registration([FromBody] RegistrationModel model)
  {
    var status = new Status();

    if (!ModelState.IsValid)
    {
      status.StatusCode = 0;
      status.Message = "Please pass all the required fields";
      return Ok(status);
    }

    // Check if Username already exists
    var userCheck = await _userManager.FindByNameAsync(model.UserName);
    if (userCheck is not null)
    {
      status.StatusCode = 0;
      status.Message = "Invalid User Name";
      return Ok(status);
    }

    var user = new ApplicationUser
    {
      UserName = model.UserName,
      SecurityStamp = Guid.NewGuid().ToString(),
      Email = model.Email,
      Name = model.Name
    };

    // Create User
    var result = await _userManager.CreateAsync(user, model.Password);
    if (!result.Succeeded)
    {
      status.StatusCode = 0;
      status.Message = "User Creation Failed";
      return Ok(status);
    }

    // add roles
    // for admin registration add UserRoles.Admin and UserRoles.User
    if (!await _roleManager.RoleExistsAsync(UserRoles.User))
      await _roleManager.CreateAsync(new IdentityRole(UserRoles.User));

    if (await _roleManager.RoleExistsAsync(UserRoles.User))
      await _userManager.AddToRoleAsync(user, UserRoles.User);

    // // Check if Email already exists
    // var emailCheck = await _userManager.FindByEmailAsync(model.Email);
    // if (emailCheck != null)
    // {
    //   status.StatusCode = 0;
    //   status.Message = "Email already exists";
    //   return Ok(status);
    // }

    status.StatusCode = 1;
    status.Message = "Successfully Registered";
    return Ok(status);
  }

  // After resisting admin we will command this code, because i want only one admin in this application
  [HttpPost]
  public async Task<IActionResult> RegistrationAdmin([FromBody] RegistrationModel model)
  {
    var status = new Status();

    if (!ModelState.IsValid)
    {
      status.StatusCode = 0;
      status.Message = "Please pass all the required fields";
      return Ok(status);
    }

    // Check if Username already exists
    var userCheck = await _userManager.FindByNameAsync(model.UserName);
    if (userCheck is not null)
    {
      status.StatusCode = 0;
      status.Message = "Invalid User Name";
      return Ok(status);
    }

    var admin = new ApplicationUser
    {
      UserName = model.UserName,
      SecurityStamp = Guid.NewGuid().ToString(),
      Email = model.Email,
      Name = model.Name
    };

    // Create User
    var result = await _userManager.CreateAsync(admin, model.Password);
    if (!result.Succeeded)
    {
      status.StatusCode = 0;
      status.Message = string.Join(", ", result.Errors.Select(e => e.Description));// "User Creation Failed";
      return Ok(status);
    }

    // add roles
    // for admin registration add UserRoles.Admin and UserRoles.User
    if (!await _roleManager.RoleExistsAsync(UserRoles.Admin))
      await _roleManager.CreateAsync(new IdentityRole(UserRoles.Admin));

    if (await _roleManager.RoleExistsAsync(UserRoles.Admin))
      await _userManager.AddToRoleAsync(admin, UserRoles.Admin);

    status.StatusCode = 1;
    status.Message = "Successfully Registered";
    return Ok(status);
  }
}