﻿using System.Security.Cryptography;
using FilewareApi.Models;
using SixLabors.ImageSharp;

namespace FilewareApi.Services.FileManagerService;

public class FileManagerService(FilewareDbContext dbContext) : IFileManagerService
{
    private const string FileStoragePath = "storage/";

    public static DateTime NowWithoutTimezone => new(DateTime.Now.Ticks);

    public int RegisterNewFile(IFormFile file)
    {
        var data = new FileData
        {
            Name = file.FileName, Version = 1, LastChange = NowWithoutTimezone, LoadTime = NowWithoutTimezone,
            Size = file.Length, FileType = file.ContentType
        };
        using var hashStream = new MemoryStream();
        file.CopyTo(hashStream);

        var readBuffer = hashStream.ToArray();

        data.Data = readBuffer;
        data.Preview = EncodePreview(data.Data, data.FileType);

        dbContext.FileData.Add(data);
        dbContext.SaveChanges();

        dbContext.HistoryPoints.Add(new HistoryPoint
            { LinkedId = data.Id, Type = (int)HistoryPointType.File, Time = NowWithoutTimezone });

        Directory.CreateDirectory("storage");

        using var fileStream = File.OpenWrite(FileStoragePath + data.Id);
        fileStream.Write(readBuffer, 0, readBuffer.Length);


        dbContext.SaveChanges();

        return data.Id;
    }

    public int RegisterBigFile(byte[] data, string name, string type)
    {
        var file = new FileData
        {
            Name = name, Version = 1, LastChange = NowWithoutTimezone, LoadTime = NowWithoutTimezone,
            Size = data.Length, FileType = type
        };


        if (data.Length > 500_000_000)
        {
            dbContext.FileData.Add(file);
            dbContext.SaveChanges();

            Directory.CreateDirectory("storage");

            using var fileStream = File.OpenWrite(FileStoragePath + file.Id);
            fileStream.Write(data, 0, data.Length);
        }
        else
        {
            file.Data = data;
            file.Preview = EncodePreview(file.Data, file.FileType);

            dbContext.FileData.Add(file);
            dbContext.SaveChanges();
        }

        dbContext.HistoryPoints.Add(new HistoryPoint
            { LinkedId = file.Id, Type = (int)HistoryPointType.File, Time = NowWithoutTimezone });
        dbContext.SaveChanges();

        return file.Id;
    }

    public long GetFileSize(int id)
    {
        var file = FileById(id);
        if (file is null)
            return -1;
        return file.Size;
    }

    public void UpdateFile(int id, IFormFile form)
    {
        var file = FileById(id);

        if (file is null)
            throw new Exception("Invalid file id");

        file.Version = file.Version + 1;
        file.LastChange = NowWithoutTimezone;
        file.Size = form.Length;
        using var fileStream = new MemoryStream();
        form.CopyTo(fileStream);
        file.Data = fileStream.ToArray();
        file.Preview = EncodePreview(file.Data, file.FileType);

        dbContext.FileData.Update(file);
        dbContext.SaveChanges();
    }

    public void UpdateBigFile(int id, byte[] data)
    {
        var file = FileById(id);

        if (file is null)
            throw new Exception("Invalid file id");
        file.Version = file.Version + 1;
        file.LastChange = NowWithoutTimezone;
        file.Size = data.Length;

        if (data.Length > 500_000_000)
        {
            Directory.CreateDirectory("storage");

            using var fileStream = File.OpenWrite(FileStoragePath + file.Id);
            fileStream.Write(data, 0, data.Length);
        }
        else
        {
            file.Data = data;
            file.Preview = EncodePreview(file.Data, file.FileType);
        }

        dbContext.FileData.Update(file);
        dbContext.SaveChanges();
    }

    public void DeleteFile(int id)
    {
        var file = FileById(id);

        if (file is null)
            throw new Exception("Invalid file id");

        dbContext.FileData.Remove(file);
        File.Delete(FileStoragePath + id);
        dbContext.HistoryPoints.Remove(dbContext.HistoryPoints.FirstOrDefault(i => i.LinkedId == file.Id));
        dbContext.SaveChanges();
    }

    public FileData? GetFileById(int id)
    {
        return FileById(id);
    }

    public Stream? GetFile(int id)
    {
        return !File.Exists(FileStoragePath + id) ? null : File.OpenRead(FileStoragePath + id);
    }

    public IReadOnlyList<FileData> GetAllFiles()
    {
        return dbContext.FileData.ToList();
    }

    public void RenameFile(int id, string name)
    {
        var file = FileById(id);

        if (file is null)
            throw new Exception("Invalid file id");

        file.Name = name;
        dbContext.SaveChanges();
    }

    public byte[]? GetFilePreview(int id)
    {
        var file = FileById(id);

        if (file is null)
            throw new Exception("Invalid file id");
        return file.Preview;
    }

    private FileData? FileById(int id) => dbContext.FileData.FirstOrDefault(i => i.Id == id);

    public byte[] EncodePreview(byte[] raw, string type)
    {
        if (!type.StartsWith("image/"))
            return null;
        if (type == "image/webp")
            return null;
        using var img = Image.Load(raw);
        using var stream = new MemoryStream();
        img.SaveAsWebp(stream);
        return stream.ToArray();
    }
}