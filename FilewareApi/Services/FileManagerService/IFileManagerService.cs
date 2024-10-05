namespace FilewareApi.Services.FileManagerService;

public interface IFileManagerService
{
    Guid RegisterNewFile(Stream file, string name);
    int GetFileSize(Guid id);
    void UpdateFile(Guid id, Stream newDataStream);
}