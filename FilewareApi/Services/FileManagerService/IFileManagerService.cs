using FilewareApi.Models;

namespace FilewareApi.Services.FileManagerService;

public interface IFileManagerService
{
    Guid RegisterNewFile(IFormFile file);
    long GetFileSize(Guid id);
    void UpdateFile(Guid id, Stream newDataStream);
    void DeleteFile(Guid id);
    FileData? GetFileById(Guid id);
    IReadOnlyList<FileData> GetAllFiles(Guid id);
    void Save();
}