using Microsoft.AspNetCore.Mvc;

namespace FilewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : Controller
{
    [HttpPost]
    public async Task<ActionResult> RegisterNewFile(IFormFile file)
    {
        Console.WriteLine(file.FileName);
        var x = file.Headers;
        file.CopyTo(Console.OpenStandardOutput());
        return Ok();
    }
}