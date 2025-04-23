using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using FilewareApi.Models;
using FilewareApi.Services.UserService;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;

namespace FilewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UserController(IUserService userService) : Controller
{
    [HttpGet("totp")]
    public ActionResult<byte[]> GetTotp(string username)
    {
        return Ok(userService.GenerateTotpKey(username));
    }

    [HttpGet("exists")]
    public ActionResult<byte[]> IsUserExists(string username)
    {
        return Ok(userService.IsExists(username));
    }

    [HttpPost("auth")]
    public ActionResult<LoginResponse> Auth(string login, string password)
    {
        if (userService.Auth(login, password) is { } loggedIn)
        {
            var claims = new List<Claim>
            {
                new(ClaimTypes.Authentication, loggedIn.Id.ToString()),
                new(ClaimTypes.Name, loggedIn.Username),
            };
            var jwt = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromHours(1)), // время действия 6 часов
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(),
                    SecurityAlgorithms.HmacSha256));

            return Ok(new LoginResponse
            {
                Token = new JwtSecurityTokenHandler().WriteToken(jwt), UserData = Utils
                    .TransferData<CommonUserData>(loggedIn).Also(
                        u => { u.FileCount = userService.GetFileCount(u.Id); })
            });
        }

        return Unauthorized();
    }

    [HttpPost("reg")]
    public async Task<ActionResult> RegisterUser(User user)
    {
        if (userService.IsExists(user.Username))
        {
            return Conflict();
        }

        var id = await userService.Register(user);
        return Ok(id);
    }

    [HttpGet("{id}")]
    public ActionResult<CommonUserData> GetUser(int id)
    {
        var user = userService.Get(id);
        if (user is null)
        {
            return NotFound();
        }

        return Ok(Utils.TransferData<CommonUserData>(user));
    }

    [HttpGet("{id}/avatar")]
    public ActionResult GetAvatar(int id)
    {
        var user = userService.Get(id);
        return File(userService.GetAvatar(id), "image/webp", $"{user.Username}_avatar.webp");
    }

    [HttpPost("{id}/avatar")]
    public async Task<ActionResult> SetAvatar(int id, IFormFile avatar)
    {
        using var ms = new MemoryStream();
        await avatar.OpenReadStream().CopyToAsync(ms);
        await userService.SetupAvatar(id, ms.ToArray());
        return Ok();
    }

    [HttpPatch]
    public async Task<ActionResult> UpdateUser(CommonUserData patching)
    {
        var user = userService.Get(patching.Id);
        if (user is null)
        {
            return NotFound();
        }

        await userService.Update(patching);
        return Ok();
    }

    [HttpGet("totp/{username}/verify")]
    public ActionResult VerifyUser(string username, string totpKey)
    {
        return Ok(userService.VerifyTotp(username, totpKey));
    }
}

public class LoginResponse
{
    public string Token { get; set; }
    public CommonUserData UserData { get; set; }
}

public class AuthOptions
{
    public const string ISSUER = "Fileware api"; // издатель токена
    public const string AUDIENCE = "Fileviewr"; // потребитель токена
    const string KEY = "6D3A3EFB-4D81-4E01-BDB0-808A64F88ECA"; // ключ для шифрации

    public static SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.UTF8.GetBytes(KEY));
}