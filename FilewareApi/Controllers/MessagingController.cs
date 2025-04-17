using FilewareApi.Models;
using FilewareApi.Services.MessagingService;
using Microsoft.AspNetCore.Mvc;

namespace FilewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MessagingController(IMessagingService messagingService) : Controller
{
    [HttpGet("{id}")]
    public ActionResult<Message?> GetMessageById(int id)
    {
        return messagingService.GetMessage(id);
    }

    [HttpPost]
    public ActionResult<int> PostMessage([FromBody] string text, string filespace)
    {
        return messagingService.PostMessage(text, filespace);
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