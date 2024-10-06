using FilewareApi.Models;

namespace FilewareApi.Services.FileManagerService;

public interface IFileManagerService
{
    Guid RegisterNewFile(IFormFile file);
    long GetFileSize(Guid id);
    void UpdateFile(Guid id, IFormFile form);
    void DeleteFile(Guid id);
    FileData? GetFileById(Guid id);
    public Stream? GetFile(Guid id);
    IReadOnlyList<FileData> GetAllFiles();
    public void RenameFile(Guid id, string name);
    void Save();
}