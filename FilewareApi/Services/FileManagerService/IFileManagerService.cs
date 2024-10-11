using FilewareApi.Models;

namespace FilewareApi.Services.FileManagerService;

public interface IFileManagerService
{
    int RegisterNewFile(IFormFile file);
    int RegisterBigFile(byte[] data, string name, string type);
    long GetFileSize(int id);
    void UpdateFile(int id, IFormFile form);
    void UpdateBigFile(int id, byte[] data);
    void DeleteFile(int id);
    FileData? GetFileById(int id);
    public Stream? GetFile(int id);
    IReadOnlyList<FileData> GetAllFiles();
    void RenameFile(int id, string name);
}