using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace FilewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HistoryController : Controller
{
    [HttpGet]
    public IActionResult GetHistory()
    {
        return Ok();
    }
}