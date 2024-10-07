using System.ComponentModel.DataAnnotations;
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
}