using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

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

    [JsonIgnore]
    public byte[]? Data { get; set; }

    [JsonIgnore]
    public byte[]? Preview { get; set; }
    public byte[]? SuperPreview { get; set; }

    [NotMapped]
    public bool HasPreview => Preview != null;
}