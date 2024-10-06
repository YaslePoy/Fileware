using System.Reflection;
using System.Text.Json;
using FilewareApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace FilewareApi.Services.FileManagerService;

public class FileManagerService : IFileManagerService
{
    private const string FileInfoPath = "FilesInfo.json";
    private const string FileStoragePath = "storage/";

    public static DateTime NowWithoutTimezone => new(DateTime.Now.Ticks);
    
    public FileManagerService()
    {
        _files = JsonSerializer.Deserialize<List<FileData>>(File.ReadAllText(FileInfoPath)) ?? [];
    }

    public Guid RegisterNewFile(IFormFile file)
    {
        var data = new FileData { Id = Guid.NewGuid(), Name = file.FileName, Version = 1, LastChange = NowWithoutTimezone, LoadTime = NowWithoutTimezone, Size = file.Length};

        _files.Add(data);
        Directory.CreateDirectory("storage");
        
        using var fileStream = File.OpenWrite(FileStoragePath + data.Id);
        file.CopyTo(fileStream);
        
        return data.Id;
    }

    public long GetFileSize(Guid id)
    {
        var file = FileById(id);
        if (file is null)
            return -1;
        return file.Size;
    }

    public void UpdateFile(Guid id, IFormFile form)
    {
        var file = FileById(id);

        if (file is null)
            throw new Exception("Invalid file id");

        file.Version++;
        file.LastChange = NowWithoutTimezone;
        file.Size = form.Length;
        using var fileStream = File.OpenWrite(FileStoragePath + id);
        form.CopyTo(fileStream);
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
        return FileById(id);
    }

    public Stream? GetFile(Guid id)
    {
        return !File.Exists(FileStoragePath + id) ? null : File.OpenRead(FileStoragePath + id);
    }

    public IReadOnlyList<FileData> GetAllFiles()
    {
        return _files;
    }

    public void RenameFile(Guid id, string name)
    {
        var file = FileById(id);

        if (file is null)
            throw new Exception("Invalid file id");

        file.Name = name;
    }

    public void Save()
    {
        File.WriteAllText(FileInfoPath, JsonSerializer.Serialize(_files, new JsonSerializerOptions { WriteIndented = true }));
    }

    private List<FileData> _files;

    private FileData? FileById(Guid id) => _files.Find(i => i.Id == id);
}