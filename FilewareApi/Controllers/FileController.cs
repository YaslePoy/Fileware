using FilewareApi.Services.FileManagerService;
using Microsoft.AspNetCore.Mvc;

namespace FilewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController : Controller
{
    private readonly IFileManagerService _fileService;

    public FileController(IFileManagerService fileService)
    {
        _fileService = fileService;
    }

    [HttpPost]
    public ActionResult RegisterNewFile(IFormFile file)
    {
        var id = new { id = _fileService.RegisterNewFile(file) };
        _fileService.Save();
        
        return Ok(id);
    }

    [HttpGet("{id}/size")]
    public ActionResult FileSize(Guid id)
    {
        var size = _fileService.GetFileSize(id);
        if (size == -1)
            return NotFound();
        return Ok(new { size });
    }

    [HttpGet("{id}")]
    public ActionResult GetFileInfo(Guid id)
    {
        var file = _fileService.GetFileById(id);
        if (file is null)
            return NotFound();
        return Ok(file);
    }
}