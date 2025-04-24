using System.Net;
using System.Security.Claims;
using System.Text;
using FilewareApi.Services.FileManagerService;
using FilewareApi.Services.UserService;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Net.Http.Headers;

namespace FilewareApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FileController(IFileManagerService fileService, IUserService userService) : Controller
{
    private FormOptions _defaultFormOptions = new();

    [HttpPost("large")]
    public async Task<IActionResult> UploadBigFile(string fileSpace)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.Authentication).Value);
        var user = userService.Get(userId);
        if (user is null || !user.AttachedFileSpaces.Contains(fileSpace))
        {
            return Unauthorized();
        }
        
        if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        {
            ModelState.AddModelError("File",
                $"The request couldn't be processed (Error 1).");
            return BadRequest(ModelState);
        }

        var formAccumulator = new KeyValueAccumulator();
        var trustedFileNameForDisplay = string.Empty;
        var untrustedFileNameForStorage = string.Empty;
        var streamedFileContent = Array.Empty<byte>();

        var boundary = MultipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(Request.ContentType),
            _defaultFormOptions.MultipartBoundaryLengthLimit);
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);

        var section = await reader.ReadNextSectionAsync();

        while (section != null)
        {
            var hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition, out var contentDisposition);

            if (hasContentDispositionHeader)
            {
                if (MultipartRequestHelper
                    .HasFileContentDisposition(contentDisposition))
                {
                    untrustedFileNameForStorage = contentDisposition.FileName.Value;
                    // Don't trust the file name sent by the client. To display
                    // the file name, HTML-encode the value.
                    trustedFileNameForDisplay = WebUtility.HtmlEncode(
                        contentDisposition.FileName.Value);

                    using var ms = new MemoryStream();
                    await section.Body.CopyToAsync(ms);
                    streamedFileContent = ms.ToArray();

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }
                }
                else if (MultipartRequestHelper
                         .HasFormDataContentDisposition(contentDisposition))
                {
                    // Don't limit the key name length because the 
                    // multipart headers length limit is already in effect.
                    var key = HeaderUtilities
                        .RemoveQuotes(contentDisposition.Name).Value;
                    var encoding = Encoding.UTF8;

                    using (var streamReader = new StreamReader(
                               section.Body,
                               encoding,
                               detectEncodingFromByteOrderMarks: true,
                               bufferSize: 1024,
                               leaveOpen: true))
                    {
                        // The value length limit is enforced by 
                        // MultipartBodyLengthLimit
                        var value = await streamReader.ReadToEndAsync();

                        if (string.Equals(value, "undefined",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            value = string.Empty;
                        }

                        formAccumulator.Append(key, value);
                        if (formAccumulator.ValueCount >
                            _defaultFormOptions.ValueCountLimit)
                        {
                            // Form key count limit of 
                            // _defaultFormOptions.ValueCountLimit 
                            // is exceeded.
                            ModelState.AddModelError("File",
                                $"The request couldn't be processed (Error 3).");
                            // Log error

                            return BadRequest(ModelState);
                        }
                    }
                }
            }

            // Drain any remaining section body that hasn't been consumed and
            // read the headers for the next section.
            section = await reader.ReadNextSectionAsync();
        }


        return Ok(fileService.RegisterBigFile(streamedFileContent, untrustedFileNameForStorage, Request.ContentType, fileSpace));
    }

    [HttpPost]
    public async Task<ActionResult> RegisterNewFile(IFormFile file, string fileSpace)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.Authentication).Value);
        var user = userService.Get(userId);
        if (user is null || !user.AttachedFileSpaces.Contains(fileSpace))
        {
            return Unauthorized();
        }

        var id = fileService.RegisterNewFile(file, fileSpace);
        return Ok(id);
    }

    [HttpGet("{id}/preview")]
    public ActionResult GetFilePreview(int id)
    {
        var preview = fileService.GetFilePreview(id);
        return File(preview, "image/webp");
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
        if (file is null)
            return NotFound();
        if (file.Data != null)
            return File(file.Data, file.FileType, file.Name);

        var stream = fileService.GetFile(id);

        if (stream is null)
            return NotFound();

        return File(stream, file.FileType, file.Name);
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

    [HttpPatch("large/{id}")]
    [DisableFormValueModelBinding]
    public async Task<ActionResult> UpdateLargeFile(int id)
    {
        var file = fileService.GetFileById(id);
        if (file is null)
            return NotFound();

        if (!MultipartRequestHelper.IsMultipartContentType(Request.ContentType))
        {
            ModelState.AddModelError("File",
                $"The request couldn't be processed (Error 1).");
            return BadRequest(ModelState);
        }

        var formAccumulator = new KeyValueAccumulator();
        var trustedFileNameForDisplay = string.Empty;
        var untrustedFileNameForStorage = string.Empty;
        var streamedFileContent = Array.Empty<byte>();

        var boundary = MultipartRequestHelper.GetBoundary(
            MediaTypeHeaderValue.Parse(Request.ContentType),
            _defaultFormOptions.MultipartBoundaryLengthLimit);
        var reader = new MultipartReader(boundary, HttpContext.Request.Body);

        var section = await reader.ReadNextSectionAsync();

        while (section != null)
        {
            var hasContentDispositionHeader =
                ContentDispositionHeaderValue.TryParse(
                    section.ContentDisposition, out var contentDisposition);

            if (hasContentDispositionHeader)
            {
                if (MultipartRequestHelper
                    .HasFileContentDisposition(contentDisposition))
                {
                    untrustedFileNameForStorage = contentDisposition.FileName.Value;
                    // Don't trust the file name sent by the client. To display
                    // the file name, HTML-encode the value.
                    trustedFileNameForDisplay = WebUtility.HtmlEncode(
                        contentDisposition.FileName.Value);

                    using var ms = new MemoryStream();
                    await section.Body.CopyToAsync(ms);
                    streamedFileContent = ms.ToArray();

                    if (!ModelState.IsValid)
                    {
                        return BadRequest(ModelState);
                    }
                }
                else if (MultipartRequestHelper
                         .HasFormDataContentDisposition(contentDisposition))
                {
                    // Don't limit the key name length because the 
                    // multipart headers length limit is already in effect.
                    var key = HeaderUtilities
                        .RemoveQuotes(contentDisposition.Name).Value;
                    var encoding = Encoding.UTF8;

                    using (var streamReader = new StreamReader(
                               section.Body,
                               encoding,
                               detectEncodingFromByteOrderMarks: true,
                               bufferSize: 1024,
                               leaveOpen: true))
                    {
                        // The value length limit is enforced by 
                        // MultipartBodyLengthLimit
                        var value = await streamReader.ReadToEndAsync();

                        if (string.Equals(value, "undefined",
                                StringComparison.OrdinalIgnoreCase))
                        {
                            value = string.Empty;
                        }

                        formAccumulator.Append(key, value);
                        if (formAccumulator.ValueCount >
                            _defaultFormOptions.ValueCountLimit)
                        {
                            // Form key count limit of 
                            // _defaultFormOptions.ValueCountLimit 
                            // is exceeded.
                            ModelState.AddModelError("File",
                                $"The request couldn't be processed (Error 3).");
                            // Log error

                            return BadRequest(ModelState);
                        }
                    }
                }
            }

            // Drain any remaining section body that hasn't been consumed and
            // read the headers for the next section.
            section = await reader.ReadNextSectionAsync();
        }

        fileService.UpdateBigFile(id, streamedFileContent);

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

public static class MultipartRequestHelper
{
    // Content-Type: multipart/form-data; boundary="----WebKitFormBoundarymx2fSWqWSd0OxQqq"
    // The spec at https://tools.ietf.org/html/rfc2046#section-5.1 states that 70 characters is a reasonable limit.
    public static string GetBoundary(MediaTypeHeaderValue contentType, int lengthLimit)
    {
        var boundary = HeaderUtilities.RemoveQuotes(contentType.Boundary).Value;

        if (string.IsNullOrWhiteSpace(boundary))
        {
            throw new InvalidDataException("Missing content-type boundary.");
        }

        if (boundary.Length > lengthLimit)
        {
            throw new InvalidDataException(
                $"Multipart boundary length limit {lengthLimit} exceeded.");
        }

        return boundary;
    }

    public static bool IsMultipartContentType(string contentType)
    {
        return !string.IsNullOrEmpty(contentType)
               && contentType.IndexOf("multipart/", StringComparison.OrdinalIgnoreCase) >= 0;
    }

    public static bool HasFormDataContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
        // Content-Disposition: form-data; name="key";
        return contentDisposition != null
               && contentDisposition.DispositionType.Equals("form-data")
               && string.IsNullOrEmpty(contentDisposition.FileName.Value)
               && string.IsNullOrEmpty(contentDisposition.FileNameStar.Value);
    }

    public static bool HasFileContentDisposition(ContentDispositionHeaderValue contentDisposition)
    {
        // Content-Disposition: form-data; name="myfile1"; filename="Misc 002.jpg"
        return contentDisposition != null
               && contentDisposition.DispositionType.Equals("form-data")
               && (!string.IsNullOrEmpty(contentDisposition.FileName.Value)
                   || !string.IsNullOrEmpty(contentDisposition.FileNameStar.Value));
    }
}

[AttributeUsage(AttributeTargets.Class | AttributeTargets.Method)]
public class DisableFormValueModelBindingAttribute : Attribute, IResourceFilter
{
    public void OnResourceExecuting(ResourceExecutingContext context)
    {
        var factories = context.ValueProviderFactories;
        factories.RemoveType<FormValueProviderFactory>();
        factories.RemoveType<FormFileValueProviderFactory>();
        factories.RemoveType<JQueryFormValueProviderFactory>();
    }

    public void OnResourceExecuted(ResourceExecutedContext context)
    {
    }
}