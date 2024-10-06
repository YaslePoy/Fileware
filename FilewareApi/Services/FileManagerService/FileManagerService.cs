using System.Reflection;
using System.Text.Json;
using FilewareApi.Models;

namespace FilewareApi.Services.FileManagerService;


public class FileManagerService : IFileManagerService
{
    private const string FileInfoPath = "FilesInfo.json";
    private const string FileStoragePath = "storage/";
    public FileManagerService()
    {
        _files = JsonSerializer.Deserialize<List<FileData>>(File.ReadAllText(FileInfoPath))  ?? [];
    }
    public Guid RegisterNewFile(IFormFile file)
    {
        var data = new FileData { Id = Guid.NewGuid(), Name = file.FileName, Version = 1, LastChange = DateTime.Now };
        
        _files.Add(data);
        Directory.CreateDirectory("storage");
        var fileStream = File.OpenWrite(FileStoragePath + data.Id);
        file.CopyTo(fileStream);
        return data.Id;
    }

    public long GetFileSize(Guid id)
    {
        var file = FileById(id);
        if (file is null)
            return -1;
        return new FileInfo(FileStoragePath + file.Id).Length;
    }

    public void UpdateFile(Guid id, Stream newDataStream)
    {
        var file = FileById(id);
        
        if (file is null)
            throw new Exception("Invalid file id");
        
        file.Version++;
        file.LastChange = DateTime.Now;
        var fileStream = File.OpenWrite(FileStoragePath + id);
        newDataStream.CopyTo(fileStream);
    }

    public void DeleteFile(Guid id)
    {
        var file = FileById(id);
        
        if (file is null)
            throw new Exception("Invalid file id");

        _files.Remove(file);
        File.Delete(FileStoragePath + id);
    }

    public FileData? GetFileById(Guid id)
    {
        return  FileById(id);
    }

    public IReadOnlyList<FileData> GetAllFiles(Guid id)
    {
        return _files;
    }

    public void Save()
    {
        File.WriteAllText(FileInfoPath, JsonSerializer.Serialize(_files));
    }

    private List<FileData> _files;

    private FileData? FileById(Guid id) => _files.Find(i => i.Id == id);
}