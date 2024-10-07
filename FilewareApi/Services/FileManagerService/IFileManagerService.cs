using FilewareApi.Models;

namespace FilewareApi.Services.FileManagerService;

public interface IFileManagerService
{
    int RegisterNewFile(IFormFile file);
    long GetFileSize(int id);
    void UpdateFile(int id, IFormFile form);
    void DeleteFile(int id);
    FileData? GetFileById(int id);
    public Stream? GetFile(int id);
    IReadOnlyList<FileData> GetAllFiles();
    public void RenameFile(int id, string name);
}