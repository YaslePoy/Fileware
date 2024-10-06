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
    
    [HttpGet("{id}/load")]
    public ActionResult GetFileData(Guid id)
    {
        var file = _fileService.GetFileById(id);
        var stream = _fileService.GetFile(id);
        
        if (stream is null)
            return NotFound();
        
        return File(stream, "application/octet-stream", file.Name);
    }
    
    [HttpDelete("{id}")]
    public ActionResult DeleteFile(Guid id)
    {
        var file = _fileService.GetFileById(id);
        if (file is null)
            return NotFound();
        
        _fileService.DeleteFile(id);
        _fileService.Save();
        
        return Ok();
    }
    
    [HttpPatch("{id}")]
    public ActionResult UpdateFile(Guid id, IFormFile form)
    {
        var file = _fileService.GetFileById(id);
        if (file is null)
            return NotFound();
        
        _fileService.UpdateFile(id, form);
        _fileService.Save();
        
        return Ok();
    }

    [HttpGet("all")]
    public ActionResult GetAllFilesData()
    {
        return Ok(_fileService.GetAllFiles());
    }

    [HttpPatch("{id}/rename")]
    public IActionResult RenameFile(Guid id, [FromBody]string name)
    {
        var file = _fileService.GetFileById(id);
        if (file is null)
            return NotFound();
        
        _fileService.RenameFile(id, name);
        _fileService.Save();
        return Ok();
    }
}