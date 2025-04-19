using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace FilewareApi.Models;

public enum HistoryPointType
{
    File,
    Message
}

public class HistoryPoint
{
    [Key]
    public int Id { get; set; }
    public int Type { get; set; }
    public int LinkedId { get; set; }
    public DateTime Time { get; set; }
    [NotMapped]
    public object Linked { get; set; }
    public string FileSpaceKey { get; set; }
    public List<string> Tags { get; set; } = [];
}