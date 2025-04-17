using FilewareApi.Models;

namespace FilewareApi.Services.FileManagerService;

public interface IFileManagerService
{
    int RegisterNewFile(IFormFile file, string filespace);
    int RegisterBigFile(byte[] data, string name, string type, string filespace);
    long GetFileSize(int id);
    void UpdateFile(int id, IFormFile form);
    void UpdateBigFile(int id, byte[] data);
    void DeleteFile(int id);
    FileData? GetFileById(int id);
    Stream? GetFile(int id);
    
    IReadOnlyList<FileData> GetAllFiles();
    void RenameFile(int id, string name);
    byte[]? GetFilePreview(int id);
}