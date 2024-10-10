using FilewareApi.Services.FileManagerService;
using Microsoft.AspNetCore.Mvc;

namespace FilewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController(IFileManagerService fileService) : Controller
{
    [HttpPost]
    public ActionResult RegisterNewFile(IFormFile file)
    {
        var id = fileService.RegisterNewFile(file);
        return Ok(id);
    }

    [HttpGet("{id}/size")]
    public ActionResult FileSize(int id)
    {
        var size = fileService.GetFileSize(id);
        if (size == -1)
            return NotFound();
        return Ok(size);
    }

    [HttpGet("{id}")]
    public ActionResult GetFileInfo(int id)
    {
        var file = fileService.GetFileById(id);
        if (file is null)
            return NotFound();
        return Ok(file);
    }

    [HttpGet("{id}/load")]
    public ActionResult GetFileData(int id)
    {
        var file = fileService.GetFileById(id);
        var stream = fileService.GetFile(id);

        if (stream is null)
            return NotFound();

        return File(stream, file?.FileType!, file?.Name);
    }

    [HttpDelete("{id}")]
    public ActionResult DeleteFile(int id)
    {
        var file = fileService.GetFileById(id);
        if (file is null)
            return NotFound();

        fileService.DeleteFile(id);

        return Ok();
    }

    [HttpPatch("{id}")]
    public ActionResult UpdateFile(int id, IFormFile form)
    {
        var file = fileService.GetFileById(id);
        if (file is null)
            return NotFound();

        fileService.UpdateFile(id, form);

        return Ok();
    }

    [HttpGet("all")]
    public ActionResult GetAllFilesData()
    {
        return Ok(fileService.GetAllFiles());
    }

    [HttpPatch("{id}/rename")]
    public IActionResult RenameFile(int id, [FromBody] string name)
    {
        var file = fileService.GetFileById(id);
        if (file is null)
            return NotFound();

        fileService.RenameFile(id, name);
        return Ok();
    }
}