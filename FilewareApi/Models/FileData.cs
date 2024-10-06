namespace FilewareApi.Models;

public class FileData
{
    public Guid Id { get; set; }
    public string Name { get; set; }
    public int Version { get; set; }
    public DateTime LastChange { get; set; }
}