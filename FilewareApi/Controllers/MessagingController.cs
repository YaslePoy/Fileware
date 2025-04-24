using System.Security.Claims;
using FilewareApi.Models;
using FilewareApi.Services.MessagingService;
using FilewareApi.Services.UserService;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FilewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagingController(IMessagingService messagingService, IUserService userService) : Controller
{
    [HttpGet("{id}")]
    public ActionResult<Message?> GetMessageById(int id)
    {
        return messagingService.GetMessage(id);
    }

    [HttpPost]
    [Authorize]
    public ActionResult<int> PostMessage([FromBody] string text, string fileSpace)
    {
        var id = int.Parse(User.FindFirst(ClaimTypes.Authentication).Value);
        var user = userService.Get(id);
        if (user is null || !user.AttachedFileSpaces.Contains(fileSpace))
        {
            return Unauthorized();
        }
        return messagingService.PostMessage(text, fileSpace);
    }

    [HttpPatch("{id}")]
    public ActionResult UpdateMessageById(int id, [FromBody] string text)
    {
        messagingService.UpdateMessage(id, text);
        return Ok();
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteMessageById(int id)
    {
        messagingService.DeleteMessage(id);

        return Ok();
    }
}