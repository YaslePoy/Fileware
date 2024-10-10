using System.ComponentModel.DataAnnotations;

namespace FilewareApi.Models;

public class FileData
{
    [Key]
    public int Id { get; set; }

    public string Name { get; set; }
    public int Version { get; set; }
    public DateTime LastChange { get; set; }
    public long Size { get; set; }
    public DateTime LoadTime { get; set; }
    public string FileType { get; set; }
    public string Hash { get; set; }
}